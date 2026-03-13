using MikroClean.Application.Dtos.Router;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Domain.MikroTik;
using Microsoft.Extensions.Logging;

namespace MikroClean.Application.Services
{
    /// <summary>
    /// Servicio para gestión de routers con encriptación automática de passwords
    /// </summary>
    public class RouterService : IRouterService
    {
        private readonly IRouterRepository _routerRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IMikroTikConnectionManager _connectionManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RouterService> _logger;

        public RouterService(
            IRouterRepository routerRepository,
            IOrganizationRepository organizationRepository,
            IEncryptionService encryptionService,
            IMikroTikConnectionManager connectionManager,
            IUnitOfWork unitOfWork,
            ILogger<RouterService> logger)
        {
            _routerRepository = routerRepository;
            _organizationRepository = organizationRepository;
            _encryptionService = encryptionService;
            _connectionManager = connectionManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<RouterDTO>>> GetRoutersByOrganizationAsync(int organizationId)
        {
            try
            {
                var routers = await _routerRepository.GetByOrganizationIdAsync(organizationId);
                var routerDtos = routers.Select(MapToDto).ToList();

                return ApiResponse<IEnumerable<RouterDTO>>.Success(
                    routerDtos,
                    $"Se encontraron {routerDtos.Count} routers"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo routers de organización {OrganizationId}", organizationId);
                return ApiResponse<IEnumerable<RouterDTO>>.Error($"Error al obtener routers: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RouterDTO>> GetRouterByIdAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<RouterDTO>.NotFound("Router no encontrado");
                }

                var routerDto = MapToDto(router);
                return ApiResponse<RouterDTO>.Success(routerDto, "Router obtenido exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo router {RouterId}", routerId);
                return ApiResponse<RouterDTO>.Error($"Error al obtener router: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RouterDTO>> CreateRouterAsync(CreateRouterDTO createDto)
        {
            try
            {
                // Validar que la organización existe
                var organization = await _organizationRepository.GetByIdAsync(createDto.OrganizationId);
                if (organization == null)
                {
                    return ApiResponse<RouterDTO>.ValidationError(
                        "Organización no encontrada",
                        new { OrganizationId = "La organización no existe" }
                    );
                }

                // Validar IP única
                var existingRouter = await _routerRepository.GetByIpAsync(createDto.Ip);
                if (existingRouter != null)
                {
                    return ApiResponse<RouterDTO>.ValidationError(
                        "Ya existe un router con esa IP",
                        new { Ip = "La dirección IP ya está registrada" }
                    );
                }

                // Encriptar password
                string encryptedPassword;
                try
                {
                    encryptedPassword = _encryptionService.Encrypt(createDto.Password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error encriptando password del router");
                    return ApiResponse<RouterDTO>.Error("Error al encriptar la contraseña del router");
                }

                var router = new Router
                {
                    Name = createDto.Name,
                    Ip = createDto.Ip,
                    User = createDto.User,
                    EncryptedPassword = encryptedPassword,
                    Model = createDto.Model,
                    Location = createDto.Location,
                    OrganizationId = createDto.OrganizationId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _routerRepository.Add(router);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Router {RouterName} creado exitosamente para organización {OrganizationId}",
                    router.Name, router.OrganizationId
                );

                var routerDto = MapToDto(router);
                return ApiResponse<RouterDTO>.Success(routerDto, "Router creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando router");
                return ApiResponse<RouterDTO>.Error($"Error al crear router: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RouterDTO>> UpdateRouterAsync(int routerId, UpdateRouterDTO updateDto)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<RouterDTO>.NotFound("Router no encontrado");
                }

                // Validar IP única (si cambió)
                if (router.Ip != updateDto.Ip)
                {
                    var existingRouter = await _routerRepository.GetByIpAsync(updateDto.Ip);
                    if (existingRouter != null && existingRouter.Id != routerId)
                    {
                        return ApiResponse<RouterDTO>.ValidationError(
                            "Ya existe otro router con esa IP",
                            new { Ip = "La dirección IP ya está en uso" }
                        );
                    }
                }

                // Actualizar campos básicos
                router.Name = updateDto.Name;
                router.Ip = updateDto.Ip;
                router.User = updateDto.User;
                router.Model = updateDto.Model;
                router.Location = updateDto.Location;
                router.IsActive = updateDto.IsActive;
                router.UpdatedAt = DateTime.UtcNow;

                // Actualizar password si se proporcionó
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    try
                    {
                        router.EncryptedPassword = _encryptionService.Encrypt(updateDto.Password);
                        
                        // Si cambió password, cerrar conexión activa para forzar reconexión
                        await _connectionManager.DisconnectRouterAsync(routerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error encriptando nueva password del router {RouterId}", routerId);
                        return ApiResponse<RouterDTO>.Error("Error al encriptar la nueva contraseña");
                    }
                }

                _routerRepository.UpdateAsync(router);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Router {RouterId} actualizado exitosamente", routerId);

                var routerDto = MapToDto(router);
                return ApiResponse<RouterDTO>.Success(routerDto, "Router actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando router {RouterId}", routerId);
                return ApiResponse<RouterDTO>.Error($"Error al actualizar router: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteRouterAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Router no encontrado");
                }

                // Cerrar conexión activa antes de eliminar
                await _connectionManager.DisconnectRouterAsync(routerId);

                // Soft delete
                router.DeletedAt = DateTime.UtcNow;
                router.IsActive = false;
                
                _routerRepository.UpdateAsync(router);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Router {RouterId} eliminado exitosamente", routerId);

                return ApiResponse<bool>.Success(true, "Router eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando router {RouterId}", routerId);
                return ApiResponse<bool>.Error($"Error al eliminar router: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> TestAndUpdateRouterStatusAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Router no encontrado");
                }

                var isConnected = await _connectionManager.TestConnectionAsync(routerId);

                if (isConnected)
                {
                    // Actualizar información del router desde el dispositivo
                    // TODO: Obtener versión, MAC address, etc.
                    await _routerRepository.UpdateLastSeenAsync(routerId, DateTime.UtcNow);
                }

                return ApiResponse<bool>.Success(
                    isConnected,
                    isConnected ? "Router conectado exitosamente" : "No se pudo conectar al router"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando conexión de router {RouterId}", routerId);
                return ApiResponse<bool>.Error($"Error al probar conexión: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RebootRouterAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Router no encontrado");
                }

                if (!router.IsActive)
                {
                    return ApiResponse<bool>.ValidationError(
                        "El router está inactivo",
                        new { IsActive = "El router debe estar activo para poder reiniciarlo" }
                    );
                }

                var result = await _connectionManager.RebootRouterAsync(routerId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Router {RouterId} reiniciado exitosamente", routerId);
                    return ApiResponse<bool>.Success(true, "Router reiniciado exitosamente. El dispositivo se está reiniciando.");
                }
                else
                {
                    _logger.LogWarning("Error al reiniciar router {RouterId}: {Error}", routerId, result.Message);
                    return ApiResponse<bool>.Error($"Error al reiniciar router: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reiniciando router {RouterId}", routerId);
                return ApiResponse<bool>.Error($"Error al reiniciar router: {ex.Message}");
            }
        }

        //public async Task<ApiResponse<IEnumerable<RouterIpPoolDTO>>> GetRouterIpPoolsAsync(int routerId)
        //{
        //    try
        //    {
        //        var router = await _routerRepository.GetByIdAsync(routerId);

        //        if(router == null || router.DeletedAt != null)
        //        {
        //            return ApiResponse<IEnumerable<RouterIpPoolDTO>>.NotFound("Router no encontrado");
        //        }

        //        if (!router.IsActive)
        //        {
        //            return ApiResponse<IEnumerable<RouterIpPoolDTO>>.ValidationError(
        //                "El router está inactivo",
        //                new { IsActive = "El router debe estar activo para obtener los IP Pools" }
        //            );
        //        }

        //    }
        //    catch (Exception e)
        //    {

        //        throw;
        //    }
        //}

        private RouterDTO MapToDto(Router router)
        {
            return new RouterDTO
            {
                Id = router.Id,
                Name = router.Name,
                Ip = router.Ip,
                User = router.User,
                IsActive = router.IsActive,
                Model = router.Model,
                Version = router.Version,
                LastSeen = router.LastSeen,
                MacAddress = router.MacAddress,
                Location = router.Location,
                OrganizationId = router.OrganizationId,
                OrganizationName = router.Organization?.Name ?? string.Empty,
                CreatedAt = router.CreatedAt
            };
        }

        
    }
}

using MikroClean.Application.Dtos.SystemRole;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;

namespace MikroClean.Application.Services
{
    public class SystemRoleService : ISystemRoleService
    {
        private readonly ISystemRoleRepository _systemRoleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SystemRoleService(
            ISystemRoleRepository systemRoleRepository,
            IUnitOfWork unitOfWork)
        {
            _systemRoleRepository = systemRoleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<SystemRoleDTO>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _systemRoleRepository.GetAllAsync();
                
                var activeRoles = roles
                    .Where(r => r.DeletedAt == null)
                    .Select(MapToDto)
                    .ToList();

                return ApiResponse<IEnumerable<SystemRoleDTO>>.Success(
                    activeRoles,
                    $"Se encontraron {activeRoles.Count} roles"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SystemRoleDTO>>.Error($"Error al obtener los roles: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SystemRoleDTO>> GetRoleByIdAsync(int id)
        {
            try
            {
                var role = await _systemRoleRepository.GetByIdAsync(id);

                if (role == null || role.DeletedAt != null)
                {
                    return ApiResponse<SystemRoleDTO>.NotFound("Rol no encontrado");
                }

                var roleDto = MapToDto(role);
                return ApiResponse<SystemRoleDTO>.Success(roleDto, "Rol obtenido exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemRoleDTO>.Error($"Error al obtener el rol: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SystemRoleDTO>> GetRoleByNameAsync(string name)
        {
            try
            {
                var role = await _systemRoleRepository.GetByNameAsync(name);

                if (role == null)
                {
                    return ApiResponse<SystemRoleDTO>.NotFound($"Rol '{name}' no encontrado");
                }

                var roleDto = MapToDto(role);
                return ApiResponse<SystemRoleDTO>.Success(roleDto, "Rol obtenido exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemRoleDTO>.Error($"Error al obtener el rol: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SystemRoleDTO>> EnsureDefaultRoleExistsAsync(string roleName)
        {
            try
            {
                var existingRole = await _systemRoleRepository.GetByNameAsync(roleName);
                
                if (existingRole != null)
                {
                    var roleDto = MapToDto(existingRole);
                    return ApiResponse<SystemRoleDTO>.Success(roleDto, $"Rol '{roleName}' ya existe");
                }

                var newRole = new SystemRole
                {
                    Name = roleName,
                    CreatedAt = DateTime.UtcNow
                };

                _systemRoleRepository.Add(newRole);
                await _unitOfWork.SaveChangesAsync();

                var createdRoleDto = MapToDto(newRole);
                return ApiResponse<SystemRoleDTO>.Success(createdRoleDto, $"Rol '{roleName}' creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemRoleDTO>.Error($"Error al crear el rol por defecto: {ex.Message}");
            }
        }

        private SystemRoleDTO MapToDto(SystemRole role)
        {
            return new SystemRoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt
            };
        }
    }
}

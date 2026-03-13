using MikroClean.Application.Dtos.License;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using System.Security.Cryptography;
using System.Text;

namespace MikroClean.Application.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ILicenseRepository _licenseRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LicenseService(
            ILicenseRepository licenseRepository,
            IOrganizationRepository organizationRepository,
            IUnitOfWork unitOfWork)
        {
            _licenseRepository = licenseRepository;
            _organizationRepository = organizationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<LicenseDTO>> CreateLicenseAsync(CreateLicenseDTO createLicenseDTO)
        {
            try
            {
                if (createLicenseDTO.EndDate <= createLicenseDTO.StartDate)
                {
                    return ApiResponse<LicenseDTO>.ValidationError(
                        "La fecha de fin debe ser posterior a la fecha de inicio",
                        new { EndDate = "Fecha de fin inválida" }
                    );
                }

                var license = new License
                {
                    Key = GenerateLicenseKey(),
                    Type = createLicenseDTO.Type,
                    StartDate = createLicenseDTO.StartDate,
                    EndDate = createLicenseDTO.EndDate,
                    IsActive = true,
                    MaxRouters = createLicenseDTO.MaxRouters,
                    MaxUsers = createLicenseDTO.MaxUsers,
                    OrganizationId = null,
                    CreatedAt = DateTime.UtcNow
                };

                _licenseRepository.Add(license);
                await _unitOfWork.SaveChangesAsync();

                var licenseDto = MapToDto(license);
                return ApiResponse<LicenseDTO>.Success(licenseDto, "Licencia creada exitosamente. Usa la Key para asociarla a una organización");
            }
            catch (Exception ex)
            {
                return ApiResponse<LicenseDTO>.Error($"Error al crear la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteLicenseAsync(int id)
        {
            try
            {
                var license = await _licenseRepository.GetByIdAsync(id);
                if (license == null || license.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Licencia no encontrada");
                }

                license.DeletedAt = DateTime.UtcNow;
                license.IsActive = false;
                _licenseRepository.UpdateAsync(license);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Licencia eliminada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al eliminar la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<LicenseDTO>>> GetAllLicensesAsync()
        {
            try
            {
                var licenses = await _licenseRepository.GetAllAsync();
                
                var activeLicenses = licenses
                    .Where(l => l.DeletedAt == null)
                    .Select(MapToDto)
                    .ToList();

                return ApiResponse<IEnumerable<LicenseDTO>>.Success(
                    activeLicenses,
                    $"Se encontraron {activeLicenses.Count} licencias"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<LicenseDTO>>.Error($"Error al obtener las licencias: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<LicenseDTO>>> GetAvailableLicensesAsync()
        {
            try
            {
                var licenses = await _licenseRepository.GetAllAsync();
                
                var availableLicenses = licenses
                    .Where(l => l.DeletedAt == null && 
                               (!l.OrganizationId.HasValue || l.OrganizationId.Value == 0) &&
                               l.IsActive &&
                               l.EndDate >= DateTime.UtcNow)
                    .Select(MapToDto)
                    .ToList();

                return ApiResponse<IEnumerable<LicenseDTO>>.Success(
                    availableLicenses,
                    $"Se encontraron {availableLicenses.Count} licencias disponibles"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<LicenseDTO>>.Error($"Error al obtener las licencias disponibles: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<LicenseDTO>>> GetExpiredLicensesAsync()
        {
            try
            {
                var expiredLicenses = await _licenseRepository.GetExpiredLicensesAsync();
                
                var licenseDtos = expiredLicenses.Select(MapToDto).ToList();

                return ApiResponse<IEnumerable<LicenseDTO>>.Success(
                    licenseDtos,
                    $"Se encontraron {licenseDtos.Count} licencias expiradas"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<LicenseDTO>>.Error($"Error al obtener las licencias expiradas: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LicenseDTO>> GetLicenseByIdAsync(int id)
        {
            try
            {
                var license = await _licenseRepository.GetByIdAsync(id);

                if (license == null || license.DeletedAt != null)
                {
                    return ApiResponse<LicenseDTO>.NotFound("Licencia no encontrada");
                }

                var licenseDto = MapToDto(license);
                return ApiResponse<LicenseDTO>.Success(licenseDto, "Licencia obtenida exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<LicenseDTO>.Error($"Error al obtener la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LicenseDTO>> GetLicenseByKeyAsync(string key)
        {
            try
            {
                var license = await _licenseRepository.GetByKeyAsync(key);

                if (license == null || license.DeletedAt != null)
                {
                    return ApiResponse<LicenseDTO>.NotFound("Licencia no encontrada");
                }

                var licenseDto = MapToDto(license);
                return ApiResponse<LicenseDTO>.Success(licenseDto, "Licencia obtenida exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<LicenseDTO>.Error($"Error al obtener la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LicenseDTO>> GetLicenseByOrganizationIdAsync(int organizationId)
        {
            try
            {
                var license = await _licenseRepository.GetByOrganizationIdAsync(organizationId);

                if (license == null)
                {
                    return ApiResponse<LicenseDTO>.NotFound("No se encontró licencia para esta organización");
                }

                var licenseDto = MapToDto(license);
                return ApiResponse<LicenseDTO>.Success(licenseDto, "Licencia obtenida exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<LicenseDTO>.Error($"Error al obtener la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<LicenseDTO>>> GetLicensesExpiringInDaysAsync(int days)
        {
            try
            {
                var expiringLicenses = await _licenseRepository.GetLicensesExpiringInDaysAsync(days);
                
                var licenseDtos = expiringLicenses.Select(MapToDto).ToList();

                return ApiResponse<IEnumerable<LicenseDTO>>.Success(
                    licenseDtos,
                    $"Se encontraron {licenseDtos.Count} licencias próximas a expirar"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<LicenseDTO>>.Error($"Error al obtener las licencias: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LicenseDTO>> UpdateLicenseAsync(int id, UpdateLicenseDTO updateLicenseDTO)
        {
            try
            {
                var license = await _licenseRepository.GetByIdAsync(id);
                if (license == null || license.DeletedAt != null)
                {
                    return ApiResponse<LicenseDTO>.NotFound("Licencia no encontrada");
                }

                if (updateLicenseDTO.EndDate <= updateLicenseDTO.StartDate)
                {
                    return ApiResponse<LicenseDTO>.ValidationError(
                        "La fecha de fin debe ser posterior a la fecha de inicio",
                        new { EndDate = "Fecha de fin inválida" }
                    );
                }

                license.Type = updateLicenseDTO.Type;
                license.StartDate = updateLicenseDTO.StartDate;
                license.EndDate = updateLicenseDTO.EndDate;
                license.IsActive = updateLicenseDTO.IsActive;
                license.MaxRouters = updateLicenseDTO.MaxRouters;
                license.MaxUsers = updateLicenseDTO.MaxUsers;
                license.UpdatedAt = DateTime.UtcNow;

                _licenseRepository.UpdateAsync(license);
                await _unitOfWork.SaveChangesAsync();

                var licenseDto = MapToDto(license);
                return ApiResponse<LicenseDTO>.Success(licenseDto, "Licencia actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<LicenseDTO>.Error($"Error al actualizar la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ValidateLicenseAsync(int organizationId)
        {
            try
            {
                var license = await _licenseRepository.GetByOrganizationIdAsync(organizationId);

                if (license == null)
                {
                    return ApiResponse<bool>.ValidationError(
                        "La organización no tiene una licencia asignada",
                        new { OrganizationId = "Sin licencia" }
                    );
                }

                if (!license.IsActive)
                {
                    return ApiResponse<bool>.ValidationError(
                        "La licencia está inactiva",
                        new { License = "Licencia inactiva" }
                    );
                }

                if (license.EndDate < DateTime.UtcNow)
                {
                    license.IsActive = false;
                    _licenseRepository.UpdateAsync(license);
                    await _unitOfWork.SaveChangesAsync();

                    return ApiResponse<bool>.ValidationError(
                        "La licencia ha expirado",
                        new { License = "Licencia expirada" }
                    );
                }

                return ApiResponse<bool>.Success(true, "Licencia válida");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al validar la licencia: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<LicenseDTO>>> GetLicensesPagedAsync(
            PaginationParams paginationParams, 
            int? filterByType = null, 
            bool? filterByStatus = null, 
            bool? filterExpired = null)
        {
            try
            {
                var licenses = await _licenseRepository.GetAllAsync();
                
                var query = licenses.Where(l => l.DeletedAt == null).AsQueryable();

                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var searchTerm = paginationParams.SearchTerm.ToLower();
                    query = query.Where(l => l.Key.ToLower().Contains(searchTerm));
                }

                if (filterByType.HasValue)
                {
                    query = query.Where(l => (int)l.Type == filterByType.Value);
                }

                if (filterByStatus.HasValue)
                {
                    query = query.Where(l => l.IsActive == filterByStatus.Value);
                }

                if (filterExpired.HasValue)
                {
                    if (filterExpired.Value)
                    {
                        query = query.Where(l => l.EndDate < DateTime.UtcNow);
                    }
                    else
                    {
                        query = query.Where(l => l.EndDate >= DateTime.UtcNow);
                    }
                }

                query = paginationParams.SortBy?.ToLower() switch
                {
                    "key" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.Key) 
                        : query.OrderBy(l => l.Key),
                    "type" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.Type) 
                        : query.OrderBy(l => l.Type),
                    "startdate" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.StartDate) 
                        : query.OrderBy(l => l.StartDate),
                    "enddate" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.EndDate) 
                        : query.OrderBy(l => l.EndDate),
                    "isactive" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.IsActive) 
                        : query.OrderBy(l => l.IsActive),
                    "createdat" => paginationParams.SortDescending 
                        ? query.OrderByDescending(l => l.CreatedAt) 
                        : query.OrderBy(l => l.CreatedAt),
                    _ => query.OrderByDescending(l => l.CreatedAt)
                };

                var totalCount = query.Count();
                var items = query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(MapToDto)
                    .ToList();

                var pagedResult = new PagedResult<LicenseDTO>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<LicenseDTO>>.Success(
                    pagedResult,
                    $"Se encontraron {totalCount} licencias"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<LicenseDTO>>.Error($"Error al obtener las licencias: {ex.Message}");
            }
        }

        private LicenseDTO MapToDto(License license)
        {
            return new LicenseDTO
            {
                Id = license.Id,
                Key = license.Key,
                Type = license.Type,
                StartDate = license.StartDate,
                EndDate = license.EndDate,
                IsActive = license.IsActive,
                MaxRouters = license.MaxRouters,
                MaxUsers = license.MaxUsers,
                OrganizationId = license.OrganizationId
            };
        }

        private string GenerateLicenseKey()
        {
            var timestamp = DateTime.UtcNow.Ticks.ToString();
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            var combined = timestamp + Convert.ToBase64String(randomBytes);
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                var key = Convert.ToBase64String(hashBytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .Substring(0, 32);
                
                return $"{key.Substring(0, 8)}-{key.Substring(8, 8)}-{key.Substring(16, 8)}-{key.Substring(24, 8)}";
            }
        }
    }
}

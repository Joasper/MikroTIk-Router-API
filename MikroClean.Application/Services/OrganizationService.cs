using MikroClean.Application.Dtos.License;
using MikroClean.Application.Dtos.Organization;
using MikroClean.Application.Dtos.User;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;

namespace MikroClean.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly ISystemRoleRepository _systemRoleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public OrganizationService(
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            ILicenseRepository licenseRepository,
            ISystemRoleRepository systemRoleRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _licenseRepository = licenseRepository;
            _systemRoleRepository = systemRoleRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<OrganizationDTO>> CreateOrganizationAsync(CreateOrganizationDTO createOrganizationDTO)
        {
            try
            {
                var existingOrg = await _organizationRepository.GetByExpressionAsync(o => o.Name == createOrganizationDTO.Name);
                if (existingOrg != null)
                {
                    return ApiResponse<OrganizationDTO>.ValidationError(
                        "Ya existe una organización con ese nombre",
                        new { Name = "El nombre ya está en uso" }
                    );
                }

                var existingEmail = await _organizationRepository.GetByExpressionAsync(o => o.Email == createOrganizationDTO.Email);
                if (existingEmail != null)
                {
                    return ApiResponse<OrganizationDTO>.ValidationError(
                        "Ya existe una organización con ese email",
                        new { Email = "El email ya está en uso" }
                    );
                }

                var organization = new Organizations
                {
                    Name = createOrganizationDTO.Name,
                    Email = createOrganizationDTO.Email,
                    Phone = createOrganizationDTO.Phone,
                    CreatedAt = DateTime.UtcNow
                };

                _organizationRepository.Add(organization);
                await _unitOfWork.SaveChangesAsync();

                var organizationDto = MapToDto(organization);
                return ApiResponse<OrganizationDTO>.Success(organizationDto, "Organización creada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganizationDTO>.Error($"Error al crear la organización: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteOrganizationAsync(int id)
        {
            try
            {
                var organization = await _organizationRepository.GetByIdAsync(id);
                if (organization == null)
                {
                    return ApiResponse<bool>.NotFound("Organización no encontrada");
                }

                organization.DeletedAt = DateTime.UtcNow;
                _organizationRepository.UpdateAsync(organization);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Organización eliminada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al eliminar la organización: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<OrganizationDTO>>> GetAllOrganizationsAsync()
        {
            try
            {
                var organizations = await _organizationRepository.GetAllAsync();

                var activeOrganizations = organizations
                    .Where(o => o.DeletedAt == null)
                    .Select(MapToDto)
                    .ToList();

                return ApiResponse<IEnumerable<OrganizationDTO>>.Success(
                    activeOrganizations,
                    $"Se encontraron {activeOrganizations.Count} organizaciones"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<OrganizationDTO>>.Error($"Error al obtener las organizaciones: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrganizationDTO>> GetOrganizationByIdAsync(int id)
        {
            try
            {
                var organization = await _organizationRepository.GetByIdAsync(id);

                if (organization == null || organization.DeletedAt != null)
                {
                    return ApiResponse<OrganizationDTO>.NotFound("Organización no encontrada");
                }

                var organizationDto = MapToDto(organization);
                return ApiResponse<OrganizationDTO>.Success(organizationDto, "Organización obtenida exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganizationDTO>.Error($"Error al obtener la organización: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrganizationDTO>> UpdateOrganizationAsync(int id, UpdateOrganizationDTO updateOrganizationDTO)
        {
            try
            {
                var organization = await _organizationRepository.GetByIdAsync(id);
                if (organization == null || organization.DeletedAt != null)
                {
                    return ApiResponse<OrganizationDTO>.NotFound("Organización no encontrada");
                }

                var existingOrg = await _organizationRepository.GetByExpressionAsync(
                    o => o.Name == updateOrganizationDTO.Name && o.Id != id
                );
                if (existingOrg != null)
                {
                    return ApiResponse<OrganizationDTO>.ValidationError(
                        "Ya existe otra organización con ese nombre",
                        new { Name = "El nombre ya está en uso" }
                    );
                }

                var existingEmail = await _organizationRepository.GetByExpressionAsync(
                    o => o.Email == updateOrganizationDTO.Email && o.Id != id
                );
                if (existingEmail != null)
                {
                    return ApiResponse<OrganizationDTO>.ValidationError(
                        "Ya existe otra organización con ese email",
                        new { Email = "El email ya está en uso" }
                    );
                }

                organization.Name = updateOrganizationDTO.Name;
                organization.Email = updateOrganizationDTO.Email;
                organization.Phone = updateOrganizationDTO.Phone;
                organization.UpdatedAt = DateTime.UtcNow;

                _organizationRepository.UpdateAsync(organization);
                await _unitOfWork.SaveChangesAsync();

                var organizationDto = MapToDto(organization);
                return ApiResponse<OrganizationDTO>.Success(organizationDto, "Organización actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganizationDTO>.Error($"Error al actualizar la organización: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrganizationWithAdminDTO>> CreateOrganizationWithAdminAsync(CreateOrganizationWithAdminDTO createDto)
        {
            try
            {
                var existingOrg = await _organizationRepository.GetByExpressionAsync(o => o.Name == createDto.OrganizationName);
                if (existingOrg != null)
                {
                    return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                        "Ya existe una organización con ese nombre",
                        new { OrganizationName = "El nombre ya está en uso" }
                    );
                }

                var existingOrgEmail = await _organizationRepository.GetByExpressionAsync(o => o.Email == createDto.OrganizationEmail);
                if (existingOrgEmail != null)
                {
                    return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                        "Ya existe una organización con ese email",
                        new { OrganizationEmail = "El email ya está en uso" }
                    );
                }

                var existingUsername = await _userRepository.GetByUsernameAsync(createDto.AdminUsername);
                if (existingUsername != null)
                {
                    return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                        "Ya existe un usuario con ese nombre de usuario",
                        new { AdminUsername = "El nombre de usuario ya está en uso" }
                    );
                }

                var existingUserEmail = await _userRepository.GetByEmailAsync(createDto.AdminEmail);
                if (existingUserEmail != null)
                {
                    return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                        "Ya existe un usuario con ese email",
                        new { AdminEmail = "El email ya está en uso" }
                    );
                }

                License? existingLicense = null;
                if (!string.IsNullOrWhiteSpace(createDto.LicenseKey))
                {
                    existingLicense = await _licenseRepository.GetByKeyAsync(createDto.LicenseKey);
                    if (existingLicense == null)
                    {
                        return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                            "La clave de licencia proporcionada no existe",
                            new { LicenseKey = "Licencia no encontrada" }
                        );
                    }

                    if (existingLicense.OrganizationId.HasValue && existingLicense.OrganizationId.Value != 0)
                    {
                        return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                            "La licencia ya está asociada a otra organización",
                            new { LicenseKey = "Licencia ya en uso" }
                        );
                    }

                    if (!existingLicense.IsActive)
                    {
                        return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                            "La licencia no está activa",
                            new { LicenseKey = "Licencia inactiva" }
                        );
                    }

                    if (existingLicense.EndDate < DateTime.UtcNow)
                    {
                        return ApiResponse<OrganizationWithAdminDTO>.ValidationError(
                            "La licencia ha expirado",
                            new { LicenseKey = "Licencia expirada" }
                        );
                    }
                }

                var adminRole = await _systemRoleRepository.GetByNameAsync("Administrador");
                if (adminRole == null)
                {
                    adminRole = new SystemRole
                    {
                        Name = "Administrador",
                        CreatedAt = DateTime.UtcNow
                    };
                    _systemRoleRepository.Add(adminRole);
                    await _unitOfWork.SaveChangesAsync();
                }

                var organization = new Organizations
                {
                    Name = createDto.OrganizationName,
                    Email = createDto.OrganizationEmail,
                    Phone = createDto.OrganizationPhone,
                    CreatedAt = DateTime.UtcNow
                };

                _organizationRepository.Add(organization);
                await _unitOfWork.SaveChangesAsync();

                License license;
                if (existingLicense != null)
                {
                    existingLicense.OrganizationId = organization.Id;
                    existingLicense.UpdatedAt = DateTime.UtcNow;
                    _licenseRepository.UpdateAsync(existingLicense);
                    await _unitOfWork.SaveChangesAsync();
                    license = existingLicense;
                }
                else
                {
                    var trialEndDate = DateTime.UtcNow.AddDays(30);
                    license = new License
                    {
                        Key = GenerateLicenseKey(),
                        Type = TypeLicense.Trial,
                        StartDate = DateTime.UtcNow,
                        EndDate = trialEndDate,
                        IsActive = true,
                        MaxRouters = 5,
                        MaxUsers = 3,
                        OrganizationId = organization.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _licenseRepository.Add(license);
                    await _unitOfWork.SaveChangesAsync();
                }

                var adminUser = new User
                {
                    Username = createDto.AdminUsername,
                    Email = createDto.AdminEmail,
                    PasswordHash = _passwordHasher.HashPassword(createDto.AdminPassword),
                    OrganizationId = organization.Id,
                    SystemRoleId = adminRole.Id,
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _userRepository.Add(adminUser);
                await _unitOfWork.SaveChangesAsync();

                var createdUser = await _userRepository.GetUserWithRoleAsync(adminUser.Id);

                var result = new OrganizationWithAdminDTO
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    Email = organization.Email,
                    Phone = organization.Phone,
                    CreatedAt = organization.CreatedAt,
                    License = new LicenseDTO
                    {
                        Id = license.Id,
                        Key = license.Key,
                        Type = license.Type,
                        StartDate = license.StartDate,
                        EndDate = license.EndDate,
                        IsActive = license.IsActive,
                        MaxRouters = license.MaxRouters,
                        MaxUsers = license.MaxUsers
                    },
                    AdminUser = new UserDTO
                    {
                        Id = createdUser!.Id,
                        Username = createdUser.Username,
                        Email = createdUser.Email,
                        IsActive = createdUser.IsActive,
                        OrganizationId = createdUser.OrganizationId,
                        OrganizationName = organization.Name,
                        SystemRoleId = createdUser.SystemRoleId,
                        SystemRoleName = createdUser.SystemRole.Name,
                        CreatedAt = createdUser.CreatedAt
                    }
                };

                var message = existingLicense != null 
                    ? "Organización creada exitosamente con usuario administrador y licencia proporcionada"
                    : "Organización creada exitosamente con usuario administrador y licencia trial de 30 días";

                return ApiResponse<OrganizationWithAdminDTO>.Success(result, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganizationWithAdminDTO>.Error($"Error al crear la organización: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<OrganizationDTO>>> GetOrganizationsPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var organizations = await _organizationRepository.GetAllAsync();
                
                var query = organizations.Where(o => o.DeletedAt == null).AsQueryable();

                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var searchTerm = paginationParams.SearchTerm.ToLower();
                    query = query.Where(o => 
                        o.Name.ToLower().Contains(searchTerm) ||
                        o.Email.ToLower().Contains(searchTerm) ||
                        o.Phone.Contains(searchTerm)
                    );
                }

                query = paginationParams.SortBy?.ToLower() switch
                {
                    "name" => paginationParams.SortDescending 
                        ? query.OrderByDescending(o => o.Name) 
                        : query.OrderBy(o => o.Name),
                    "email" => paginationParams.SortDescending 
                        ? query.OrderByDescending(o => o.Email) 
                        : query.OrderBy(o => o.Email),
                    "createdat" => paginationParams.SortDescending 
                        ? query.OrderByDescending(o => o.CreatedAt) 
                        : query.OrderBy(o => o.CreatedAt),
                    _ => query.OrderByDescending(o => o.CreatedAt)
                };

                var totalCount = query.Count();
                var items = query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(MapToDto)
                    .ToList();

                var pagedResult = new PagedResult<OrganizationDTO>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<OrganizationDTO>>.Success(
                    pagedResult,
                    $"Se encontraron {totalCount} organizaciones"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<OrganizationDTO>>.Error($"Error al obtener las organizaciones: {ex.Message}");
            }
        }

        private OrganizationDTO MapToDto(Organizations organization)
        {
            return new OrganizationDTO
            {
                Id = organization.Id,
                Name = organization.Name,
                Email = organization.Email,
                Phone = organization.Phone,
                CreatedAt = organization.CreatedAt,
                License = organization.License != null ? new LicenseDTO
                {
                    Id = organization.License.Id,
                    Key = organization.License.Key,
                    Type = organization.License.Type,
                    StartDate = organization.License.StartDate,
                    EndDate = organization.License.EndDate,
                    IsActive = organization.License.IsActive,
                    MaxRouters = organization.License.MaxRouters,
                    MaxUsers = organization.License.MaxUsers
                } : null
            };
        }

        private string GenerateLicenseKey()
        {
            var timestamp = DateTime.UtcNow.Ticks.ToString();
            var randomBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            var combined = timestamp + Convert.ToBase64String(randomBytes);
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
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

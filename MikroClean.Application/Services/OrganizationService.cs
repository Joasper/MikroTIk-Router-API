using MikroClean.Application.Dtos.License;
using MikroClean.Application.Dtos.Organization;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;

namespace MikroClean.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrganizationService(
            IOrganizationRepository organizationRepository,
            IUnitOfWork unitOfWork)
        {
            _organizationRepository = organizationRepository;
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
    }
}

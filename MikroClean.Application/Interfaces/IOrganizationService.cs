using MikroClean.Application.Dtos.Organization;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<ApiResponse<OrganizationDTO>> GetOrganizationByIdAsync(int id);
        Task<ApiResponse<IEnumerable<OrganizationDTO>>> GetAllOrganizationsAsync();
        Task<ApiResponse<PagedResult<OrganizationDTO>>> GetOrganizationsPagedAsync(PaginationParams paginationParams);
        Task<ApiResponse<OrganizationDTO>> CreateOrganizationAsync(CreateOrganizationDTO createOrganizationDTO);
        Task<ApiResponse<OrganizationWithAdminDTO>> CreateOrganizationWithAdminAsync(CreateOrganizationWithAdminDTO createDto);
        Task<ApiResponse<OrganizationDTO>> UpdateOrganizationAsync(int id, UpdateOrganizationDTO updateOrganizationDTO);
        Task<ApiResponse<bool>> DeleteOrganizationAsync(int id);
    }
}

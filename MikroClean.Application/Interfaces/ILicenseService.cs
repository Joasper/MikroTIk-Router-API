using MikroClean.Application.Dtos.License;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface ILicenseService
    {
        Task<ApiResponse<LicenseDTO>> GetLicenseByIdAsync(int id);
        Task<ApiResponse<LicenseDTO>> GetLicenseByKeyAsync(string key);
        Task<ApiResponse<LicenseDTO>> GetLicenseByOrganizationIdAsync(int organizationId);
        Task<ApiResponse<IEnumerable<LicenseDTO>>> GetAllLicensesAsync();
        Task<ApiResponse<IEnumerable<LicenseDTO>>> GetAvailableLicensesAsync();
        Task<ApiResponse<PagedResult<LicenseDTO>>> GetLicensesPagedAsync(
            PaginationParams paginationParams, 
            int? filterByType = null, 
            bool? filterByStatus = null, 
            bool? filterExpired = null);
        Task<ApiResponse<IEnumerable<LicenseDTO>>> GetExpiredLicensesAsync();
        Task<ApiResponse<IEnumerable<LicenseDTO>>> GetLicensesExpiringInDaysAsync(int days);
        Task<ApiResponse<LicenseDTO>> CreateLicenseAsync(CreateLicenseDTO createLicenseDTO);
        Task<ApiResponse<LicenseDTO>> UpdateLicenseAsync(int id, UpdateLicenseDTO updateLicenseDTO);
        Task<ApiResponse<bool>> DeleteLicenseAsync(int id);
        Task<ApiResponse<bool>> ValidateLicenseAsync(int organizationId);
    }
}

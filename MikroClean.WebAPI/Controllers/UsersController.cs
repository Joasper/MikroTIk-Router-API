using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.User;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene todos los usuarios activos
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene usuarios con paginación, búsqueda y ordenamiento
        /// </summary>
        /// <param name="pageNumber">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 10, max: 100)</param>
        /// <param name="sortBy">Campo para ordenar (username, email, createdAt, lastLogin)</param>
        /// <param name="sortDescending">Orden descendente (default: false)</param>
        /// <param name="searchTerm">Término de búsqueda (busca en username, email)</param>
        /// <returns>Resultado paginado de usuarios</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDTO>>), 200)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? searchTerm = null)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending,
                SearchTerm = searchTerm
            };

            var response = await _userService.GetUsersPagedAsync(paginationParams);
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            return HandleResponse(response);
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<IActionResult> GetUsersByOrganization(int organizationId)
        {
            var response = await _userService.GetUsersByOrganizationAsync(organizationId);
            return HandleResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _userService.CreateUserAsync(createUserDTO);
            return HandleResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _userService.UpdateUserAsync(id, updateUserDTO);
            return HandleResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserAsync(id);
            return HandleResponse(response);
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            return HandleResponse(response);
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

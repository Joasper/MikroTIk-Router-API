using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.SystemRole;
using MikroClean.Application.Interfaces;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class SystemRolesController : BaseApiController
    {
        private readonly ISystemRoleService _systemRoleService;

        public SystemRolesController(ISystemRoleService systemRoleService)
        {
            _systemRoleService = systemRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _systemRoleService.GetAllRolesAsync();
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var response = await _systemRoleService.GetRoleByIdAsync(id);
            return HandleResponse(response);
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            var response = await _systemRoleService.GetRoleByNameAsync(name);
            return HandleResponse(response);
        }

        [HttpPost("ensure-default/{roleName}")]
        public async Task<IActionResult> EnsureDefaultRole(string roleName)
        {
            var response = await _systemRoleService.EnsureDefaultRoleExistsAsync(roleName);
            return HandleResponse(response);
        }
    }
}

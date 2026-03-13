using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Auth;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Inicia sesión con usuario y contraseña
        /// </summary>
        /// <param name="loginRequest">Credenciales de acceso</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), 400)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _authService.LoginAsync(loginRequest);
            
            if (response.Status == "success" && response.Data != null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = response.Data.ExpiresAt
                };
                
                Response.Cookies.Append("authToken", response.Data.Token, cookieOptions);
                
                Response.Cookies.Append("userId", response.Data.User.Id.ToString(), new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = response.Data.ExpiresAt
                });
            }
            
            return HandleResponse(response);
        }

        /// <summary>
        /// Valida si un token JWT es válido
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost("validate-token")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var response = await _authService.ValidateTokenAsync(token);
            return HandleResponse(response);
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Confirmación de cierre de sesión</returns>
        [HttpPost("logout/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> Logout(int userId)
        {
            var response = await _authService.LogoutAsync(userId);
            
            if (response.Status == "success")
            {
                Response.Cookies.Delete("authToken");
                Response.Cookies.Delete("userId");
            }
            
            return HandleResponse(response);
        }
    }
}

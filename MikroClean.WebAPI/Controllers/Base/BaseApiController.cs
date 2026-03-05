using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Models;

namespace MikroClean.WebAPI.Controllers.Base
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Convierte ApiResponse en IActionResult con el código HTTP apropiado
        /// </summary>
        protected IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            return response.Status switch
            {
                ResponseStatus.Success => Ok(response),
                ResponseStatus.NotFound => NotFound(response),
                ResponseStatus.ValidationError => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
                ResponseStatus.Forbidden => StatusCode(403, response),
                ResponseStatus.Error => StatusCode(500, response),
                _ => StatusCode(500, response)
            };
        }

        /// <summary>
        /// Convierte errores de ModelState en ApiResponse
        /// </summary>
        protected IActionResult HandleValidationError(string message = "Datos de entrada inválidos")
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new ApiResponse<object>
            {
                Status = ResponseStatus.ValidationError,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };

            return BadRequest(response);
        }
    }
}

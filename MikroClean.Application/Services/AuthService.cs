using MikroClean.Application.Dtos.Auth;
using MikroClean.Application.Dtos.User;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;

namespace MikroClean.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AuthService(
            IUserRepository userRepository,
            ILicenseRepository licenseRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _licenseRepository = licenseRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest)
        {
            try
            {
                var user = await _userRepository.GetByUsernameOrEmailAsync(loginRequest.UsernameOrEmail);

                if (user == null)
                {
                    return ApiResponse<LoginResponseDTO>.ValidationError(
                        "Credenciales inválidas",
                        new { Credentials = "Usuario o contraseña incorrectos" }
                    );
                }

                if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                {
                    var minutesRemaining = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                    return ApiResponse<LoginResponseDTO>.ValidationError(
                        $"Cuenta bloqueada. Intente nuevamente en {minutesRemaining} minutos",
                        new { Account = "Cuenta bloqueada temporalmente" }
                    );
                }

                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponseDTO>.ValidationError(
                        "La cuenta está desactivada",
                        new { Account = "Cuenta desactivada" }
                    );
                }

                if (user.OrganizationId.HasValue)
                {
                    var license = await _licenseRepository.GetByOrganizationIdAsync(user.OrganizationId.Value);
                    if (license != null)
                    {
                        if (!license.IsActive)
                        {
                            return ApiResponse<LoginResponseDTO>.ValidationError(
                                "La licencia de su organización está inactiva",
                                new { License = "Licencia inactiva" }
                            );
                        }

                        if (license.EndDate < DateTime.UtcNow)
                        {
                            license.IsActive = false;
                            _licenseRepository.UpdateAsync(license);
                            await _unitOfWork.SaveChangesAsync();

                            return ApiResponse<LoginResponseDTO>.ValidationError(
                                "La licencia de su organización ha expirado",
                                new { License = "Licencia expirada" }
                            );
                        }
                    }
                }

                if (!_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
                {
                    user.FailedLoginAttempts++;

                    if (user.FailedLoginAttempts >= MaxFailedAttempts)
                    {
                        user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                        _userRepository.UpdateAsync(user);
                        await _unitOfWork.SaveChangesAsync();

                        return ApiResponse<LoginResponseDTO>.ValidationError(
                            $"Cuenta bloqueada por {LockoutMinutes} minutos debido a múltiples intentos fallidos",
                            new { Account = "Cuenta bloqueada temporalmente" }
                        );
                    }

                    _userRepository.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();

                    var attemptsRemaining = MaxFailedAttempts - user.FailedLoginAttempts;
                    return ApiResponse<LoginResponseDTO>.ValidationError(
                        $"Credenciales inválidas. {attemptsRemaining} intentos restantes",
                        new { Credentials = "Usuario o contraseña incorrectos" }
                    );
                }

                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
                user.LastLogin = DateTime.UtcNow;
                _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var token = _jwtTokenService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                var response = new LoginResponseDTO
                {
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        LastLogin = user.LastLogin,
                        OrganizationId = user.OrganizationId,
                        OrganizationName = user.Organization?.Name,
                        SystemRoleId = user.SystemRoleId,
                        SystemRoleName = user.SystemRole.Name,
                        CreatedAt = user.CreatedAt
                    },
                    Token = token,
                    ExpiresAt = expiresAt
                };

                return ApiResponse<LoginResponseDTO>.Success(response, "Inicio de sesión exitoso");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDTO>.Error($"Error al iniciar sesión: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.NotFound("Usuario no encontrado");
                }

                return ApiResponse<bool>.Success(true, "Sesión cerrada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al cerrar sesión: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ValidateTokenAsync(string token)
        {
            try
            {
                var isValid = _jwtTokenService.ValidateToken(token);

                if (!isValid)
                {
                    return ApiResponse<bool>.ValidationError(
                        "Token inválido o expirado",
                        new { Token = "Token no válido" }
                    );
                }

                return ApiResponse<bool>.Success(true, "Token válido");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al validar el token: {ex.Message}");
            }
        }
    }
}

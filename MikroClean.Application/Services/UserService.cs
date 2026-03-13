using MikroClean.Application.Dtos.User;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;

namespace MikroClean.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISystemRoleRepository _systemRoleRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IUserRepository userRepository,
            ISystemRoleRepository systemRoleRepository,
            IOrganizationRepository organizationRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _systemRoleRepository = systemRoleRepository;
            _organizationRepository = organizationRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<UserDTO>> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            try
            {
                var existingUsername = await _userRepository.GetByUsernameAsync(createUserDTO.Username);
                if (existingUsername != null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "Ya existe un usuario con ese nombre de usuario",
                        new { Username = "El nombre de usuario ya está en uso" }
                    );
                }

                var existingEmail = await _userRepository.GetByEmailAsync(createUserDTO.Email);
                if (existingEmail != null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "Ya existe un usuario con ese email",
                        new { Email = "El email ya está en uso" }
                    );
                }

                var systemRole = await _systemRoleRepository.GetByIdAsync(createUserDTO.SystemRoleId);
                if (systemRole == null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "El rol del sistema no existe",
                        new { SystemRoleId = "Rol no encontrado" }
                    );
                }

                if (createUserDTO.OrganizationId.HasValue)
                {
                    var organization = await _organizationRepository.GetByIdAsync(createUserDTO.OrganizationId.Value);
                    if (organization == null)
                    {
                        return ApiResponse<UserDTO>.ValidationError(
                            "La organización no existe",
                            new { OrganizationId = "Organización no encontrada" }
                        );
                    }
                }

                var user = new User
                {
                    Username = createUserDTO.Username,
                    Email = createUserDTO.Email,
                    PasswordHash = _passwordHasher.HashPassword(createUserDTO.Password),
                    OrganizationId = createUserDTO.OrganizationId,
                    SystemRoleId = createUserDTO.SystemRoleId,
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _userRepository.Add(user);
                await _unitOfWork.SaveChangesAsync();

                var createdUser = await _userRepository.GetUserWithRoleAsync(user.Id);
                var userDto = MapToDto(createdUser!);
                
                return ApiResponse<UserDTO>.Success(userDto, "Usuario creado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDTO>.Error($"Error al crear el usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Usuario no encontrado");
                }

                user.DeletedAt = DateTime.UtcNow;
                _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Usuario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al eliminar el usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<UserDTO>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                
                var activeUsers = users
                    .Where(u => u.DeletedAt == null)
                    .Select(MapToDto)
                    .ToList();

                return ApiResponse<IEnumerable<UserDTO>>.Success(
                    activeUsers,
                    $"Se encontraron {activeUsers.Count} usuarios"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<UserDTO>>.Error($"Error al obtener los usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDTO>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetUserWithRoleAsync(id);

                if (user == null)
                {
                    return ApiResponse<UserDTO>.NotFound("Usuario no encontrado");
                }

                var userDto = MapToDto(user);
                return ApiResponse<UserDTO>.Success(userDto, "Usuario obtenido exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDTO>.Error($"Error al obtener el usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<UserDTO>>> GetUsersByOrganizationAsync(int organizationId)
        {
            try
            {
                var users = await _userRepository.GetUsersByOrganizationAsync(organizationId);
                
                var userDtos = users.Select(MapToDto).ToList();

                return ApiResponse<IEnumerable<UserDTO>>.Success(
                    userDtos,
                    $"Se encontraron {userDtos.Count} usuarios para la organización"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<UserDTO>>.Error($"Error al obtener los usuarios: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserDTO>> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.DeletedAt != null)
                {
                    return ApiResponse<UserDTO>.NotFound("Usuario no encontrado");
                }

                var existingUsername = await _userRepository.GetByExpressionAsync(
                    u => u.Username == updateUserDTO.Username && u.Id != id
                );
                if (existingUsername != null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "Ya existe otro usuario con ese nombre de usuario",
                        new { Username = "El nombre de usuario ya está en uso" }
                    );
                }

                var existingEmail = await _userRepository.GetByExpressionAsync(
                    u => u.Email == updateUserDTO.Email && u.Id != id
                );
                if (existingEmail != null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "Ya existe otro usuario con ese email",
                        new { Email = "El email ya está en uso" }
                    );
                }

                var systemRole = await _systemRoleRepository.GetByIdAsync(updateUserDTO.SystemRoleId);
                if (systemRole == null)
                {
                    return ApiResponse<UserDTO>.ValidationError(
                        "El rol del sistema no existe",
                        new { SystemRoleId = "Rol no encontrado" }
                    );
                }

                if (updateUserDTO.OrganizationId.HasValue)
                {
                    var organization = await _organizationRepository.GetByIdAsync(updateUserDTO.OrganizationId.Value);
                    if (organization == null)
                    {
                        return ApiResponse<UserDTO>.ValidationError(
                            "La organización no existe",
                            new { OrganizationId = "Organización no encontrada" }
                        );
                    }
                }

                user.Username = updateUserDTO.Username;
                user.Email = updateUserDTO.Email;
                user.OrganizationId = updateUserDTO.OrganizationId;
                user.SystemRoleId = updateUserDTO.SystemRoleId;
                user.IsActive = updateUserDTO.IsActive;
                user.UpdatedAt = DateTime.UtcNow;

                _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var updatedUser = await _userRepository.GetUserWithRoleAsync(user.Id);
                var userDto = MapToDto(updatedUser!);
                
                return ApiResponse<UserDTO>.Success(userDto, "Usuario actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDTO>.Error($"Error al actualizar el usuario: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int id, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Usuario no encontrado");
                }

                if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return ApiResponse<bool>.ValidationError(
                        "La contraseña actual es incorrecta",
                        new { CurrentPassword = "Contraseña incorrecta" }
                    );
                }

                user.PasswordHash = _passwordHasher.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Contraseña actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Error($"Error al cambiar la contraseña: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<UserDTO>>> GetUsersPagedAsync(PaginationParams paginationParams)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                
                var query = users.Where(u => u.DeletedAt == null).AsQueryable();

                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var searchTerm = paginationParams.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.Username.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm)
                    );
                }

                query = paginationParams.SortBy?.ToLower() switch
                {
                    "username" => paginationParams.SortDescending 
                        ? query.OrderByDescending(u => u.Username) 
                        : query.OrderBy(u => u.Username),
                    "email" => paginationParams.SortDescending 
                        ? query.OrderByDescending(u => u.Email) 
                        : query.OrderBy(u => u.Email),
                    "createdat" => paginationParams.SortDescending 
                        ? query.OrderByDescending(u => u.CreatedAt) 
                        : query.OrderBy(u => u.CreatedAt),
                    "lastlogin" => paginationParams.SortDescending 
                        ? query.OrderByDescending(u => u.LastLogin ?? DateTime.MinValue) 
                        : query.OrderBy(u => u.LastLogin ?? DateTime.MinValue),
                    _ => query.OrderByDescending(u => u.CreatedAt)
                };

                var totalCount = query.Count();
                var items = query
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .Select(MapToDto)
                    .ToList();

                var pagedResult = new PagedResult<UserDTO>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<UserDTO>>.Success(
                    pagedResult,
                    $"Se encontraron {totalCount} usuarios"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<UserDTO>>.Error($"Error al obtener los usuarios: {ex.Message}");
            }
        }

        private UserDTO MapToDto(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name,
                SystemRoleId = user.SystemRoleId,
                SystemRoleName = user.SystemRole?.Name ?? string.Empty,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

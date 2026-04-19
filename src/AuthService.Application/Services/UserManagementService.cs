using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class UserManagementService(IUserRepository users, IRoleRepository roles, ICloudinaryService cloudinary) : IUserManagementService
{
    public async Task<UserResponseDto> UpdateUserRoleAsync(string userId, string roleName)
    {
        roleName = roleName?.Trim() ?? string.Empty;

        // Validar inputs
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("Invalid userId", nameof(userId));
        if (!RoleConstants.AllowedRoles.Contains(roleName))
            throw new InvalidOperationException(
                $"Rol no permitido. Debe ser uno de: {string.Join(", ", RoleConstants.AllowedRoles)}");

        var user = await users.GetByIdAsync(userId);

        // Si se está degradando a un SuperAdmin, evitar eliminar al último SuperAdmin
        var isUserSuperAdmin = user.UserRoles.Any(r => r.Role.Name == RoleConstants.SUPER_ADMIN);
        if (isUserSuperAdmin && roleName != RoleConstants.SUPER_ADMIN)
        {
            var superAdminCount = await roles.CountUsersInRoleAsync(RoleConstants.SUPER_ADMIN);

            if (superAdminCount <= 1)
            {
                throw new InvalidOperationException("No se puede remover al último SuperAdmin del sistema");
            }
        }

        var role = await roles.GetByNameAsync(roleName)
                       ?? throw new InvalidOperationException($"Rol '{roleName}' no encontrado en la base de datos");

        await users.UpdateUserRoleAsync(userId, role.Id);

        user = await users.GetByIdAsync(userId);

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = cloudinary.GetFullImageUrl(user.UserProfile?.ProfilePicture ?? string.Empty),
            Phone = user.UserProfile?.Phone ?? string.Empty,
            Role = role.Name,
            Status = user.Status,
            IsEmailVerified = user.UserEmail?.EmailVerified ?? false,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(string userId)
    {
        var roleNames = await roles.GetUserRoleNamesAsync(userId);
        return roleNames;
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetUsersByRoleAsync(string roleName)
    {
        roleName = roleName?.Trim() ?? string.Empty;
        var usersInRole = await roles.GetUsersByRoleAsync(roleName);
        return usersInRole.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            Username = u.Username,
            Email = u.Email,
            ProfilePicture = cloudinary.GetFullImageUrl(u.UserProfile?.ProfilePicture ?? string.Empty),
            Phone = u.UserProfile?.Phone ?? string.Empty,
            Role = roleName,
            Status = u.Status,
            IsEmailVerified = u.UserEmail?.EmailVerified ?? false,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();
    }
}

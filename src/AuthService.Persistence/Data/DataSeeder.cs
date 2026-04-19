using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Persistence.Data;

/// <summary>
/// Seed inicial para Bite&Go: crea los roles del dominio y un SuperAdmin por defecto.
///
/// Roles sembrados:
///   - SuperAdmin
///   - Admin_Restaurante
///   - Mesero
///   - Cocinero
///   - Cliente
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHashService? passwordHasher = null)
    {
        foreach (var roleName in RoleConstants.AllowedRoles)
        {
            var exists = await context.Roles.AnyAsync(r => r.Name == roleName);
            if (!exists)
            {
                await context.Roles.AddAsync(new Role
                {
                    Id = UuidGenerator.GenerateRoleId(),
                    Name = roleName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        if (!await context.Users.AnyAsync())
        {
            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleConstants.SUPER_ADMIN);

            if (superAdminRole != null)
            {
                var hasher = passwordHasher ?? new PasswordHashService();

                var userId = UuidGenerator.GenerateUserId();
                var profileId = UuidGenerator.GenerateUserId();
                var emailId = UuidGenerator.GenerateUserId();
                var passwordResetId = UuidGenerator.GenerateUserId();
                var userRoleId = UuidGenerator.GenerateUserId();

                var superAdmin = new User
                {
                    Id = userId,
                    Name = "Super",
                    Surname = "Admin",
                    Username = "superadmin",
                    Email = "superadmin@bitego.local",
                    Password = hasher.HashPassword("BiteGo1234!"),
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserProfile = new UserProfile
                    {
                        Id = profileId,
                        UserId = userId,
                        ProfilePicture = string.Empty,
                        Phone = "00000000"
                    },
                    UserEmail = new UserEmail
                    {
                        Id = emailId,
                        UserId = userId,
                        EmailVerified = true,
                        EmailVerificationToken = null,
                        EmailVerificationTokenExpiry = null
                    },
                    UserPasswordReset = new UserPasswordReset
                    {
                        Id = passwordResetId,
                        UserId = userId,
                        PasswordResetToken = null,
                        PasswordResetTokenExpiry = null
                    },
                    UserRoles =
                    [
                        new UserRole
                        {
                            Id = userRoleId,
                            UserId = userId,
                            RoleId = superAdminRole.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    ]
                };

                await context.Users.AddAsync(superAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}

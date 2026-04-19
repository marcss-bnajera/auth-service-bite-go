// Espacio de trabajo
namespace AuthService.Domain.Enums;

/// <summary>
/// Enumeración de roles de usuario para Bite&Go.
/// El valor numérico se conserva por retro-compatibilidad pero el
/// claim "role" del JWT siempre viaja con el nombre string de RoleConstants.
/// </summary>
public enum UserRole
{
    Cliente = 0,
    Mesero = 1,
    Cocinero = 2,
    AdminRestaurante = 3,
    SuperAdmin = 4
}

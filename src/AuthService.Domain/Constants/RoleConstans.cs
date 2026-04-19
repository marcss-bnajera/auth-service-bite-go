namespace AuthService.Domain.Constants;

/// <summary>
/// Roles soportados por Bite&Go.
/// Estos nombres son los que viajan en el claim "role" del JWT y los que
/// los microservicios Bite-go-admin y Bite-go-user esperan recibir.
/// </summary>
public static class RoleConstants
{
    public const string SUPER_ADMIN = "SuperAdmin";
    public const string ADMIN_RESTAURANTE = "Admin_Restaurante";
    public const string MESERO = "Mesero";
    public const string COCINERO = "Cocinero";
    public const string CLIENTE = "Cliente";

    public static readonly string[] AllowedRoles =
    [
        SUPER_ADMIN,
        ADMIN_RESTAURANTE,
        MESERO,
        COCINERO,
        CLIENTE
    ];

    /// <summary>
    /// Rol por defecto que se asigna cuando un usuario se registra desde el frontend público.
    /// </summary>
    public const string DEFAULT_ROLE = CLIENTE;
    public const string ADMIN_ROLE = SUPER_ADMIN;
    public const string USER_ROLE = CLIENTE;
}

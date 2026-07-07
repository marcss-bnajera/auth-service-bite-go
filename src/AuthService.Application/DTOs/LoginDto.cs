using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs;

public class LoginDto
{
    [Required]
    [MaxLength(150)]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
}
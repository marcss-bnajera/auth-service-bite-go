using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Email;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController(IAuthService authService) : ControllerBase
{
    [HttpPost("create-user")]
    public async Task<ActionResult<RegisterResponseDto>> CreateUser([FromBody] AdminCreateUserDto dto)
    {
        var result = await authService.AdminCreateUserAsync(dto);
        return StatusCode(201, result);
    }

    [HttpPost("verify-user")]
    public async Task<ActionResult<EmailResponseDto>> VerifyUser([FromBody] AdminVerifyUserDto dto)
    {
        var result = await authService.AdminVerifyEmailAsync(dto.Email);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}

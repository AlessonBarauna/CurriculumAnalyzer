using CurriculumAnalyzer.API.Models.Dto;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurriculumAnalyzer.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }
}

using System.ComponentModel.DataAnnotations;

namespace CurriculumAnalyzer.API.Models.Dto;

public record RegisterRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, string Name, string Email);

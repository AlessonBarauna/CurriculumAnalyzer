using CurriculumAnalyzer.API.Models.Dto;

namespace CurriculumAnalyzer.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

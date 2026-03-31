using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Data.Entities;
using CurriculumAnalyzer.API.Exceptions;
using CurriculumAnalyzer.API.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CurriculumAnalyzer.API.Services;

public class AuthService(AppDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new ValidationException("E-mail já cadastrado.");

        var user = new UserEntity
        {
            Name = request.Name,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return new AuthResponse(GenerateToken(user), user.Name, user.Email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ValidationException("E-mail ou senha inválidos.");

        return new AuthResponse(GenerateToken(user), user.Name, user.Email);
    }

    private string GenerateToken(UserEntity user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiration = int.TryParse(configuration["Jwt:ExpirationHours"], out var hours) ? hours : 24;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

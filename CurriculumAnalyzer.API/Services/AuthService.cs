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
        var key = ResolveJwtKey();
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

    public async Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new NotFoundException("Usuário não encontrado.");

        user.Name = request.Name.Trim();
        await dbContext.SaveChangesAsync();

        return new AuthResponse(GenerateToken(user), user.Name, user.Email);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ValidationException("Senha atual incorreta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await dbContext.SaveChangesAsync();
    }

    // Chave única usada tanto para assinar (aqui) quanto para validar (Program.cs).
    // Fallback de desenvolvimento garante mínimo de 32 bytes exigido pelo HS256.
    private string ResolveJwtKey()
    {
        var configured = configuration["Jwt:Key"];
        if (!string.IsNullOrWhiteSpace(configured) && configured != "CONFIGURE_VIA_ENV")
            return configured;
        return "dev-only-key-change-in-production-32c";
    }
}

using System.Text;
using CurriculumAnalyzer.API.Data;
using CurriculumAnalyzer.API.Middleware;
using CurriculumAnalyzer.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(",")
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var databaseUrl = builder.Configuration["DATABASE_URL"]
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Produção (Railway): PostgreSQL persistente
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(databaseUrl));
}
else
{
    // Desenvolvimento local: SQLite
    var connectionString = builder.Configuration.GetConnectionString("PostgresLocal")
        ?? "Host=localhost;Port=5432;Database=curriculum_analyzer;Username=postgres;Password=postgres";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var rawJwtKey = builder.Configuration["Jwt:Key"];
var jwtKey = (!string.IsNullOrWhiteSpace(rawJwtKey) && rawJwtKey != "CONFIGURE_VIA_ENV")
    ? rawJwtKey
    : "dev-only-key-change-in-production-32c";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGrokAiService>(sp =>
    new GrokAiService(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<GrokAiService>>()));
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<ICurriculumAnalysisService, CurriculumAnalysisService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

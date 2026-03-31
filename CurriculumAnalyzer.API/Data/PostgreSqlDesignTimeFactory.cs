using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CurriculumAnalyzer.API.Data;

/// <summary>
/// Usado apenas por 'dotnet ef migrations add' para gerar migrations PostgreSQL.
/// Não afeta o comportamento em runtime.
/// </summary>
public class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=curriculum_analyzer;Username=postgres;Password=postgres",
            npg => npg.MigrationsHistoryTable("__EFMigrationsHistory"));
        return new AppDbContext(optionsBuilder.Options);
    }
}

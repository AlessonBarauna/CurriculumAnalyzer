using CurriculumAnalyzer.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CurriculumEntity> Curriculums { get; set; }
    public DbSet<AnalysisEntity> Analyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalysisEntity>()
            .HasOne(a => a.Curriculum)
            .WithMany(c => c.Analyses)
            .HasForeignKey(a => a.CurriculumId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CurriculumEntity>().HasIndex(c => c.UploadedAt);
        modelBuilder.Entity<AnalysisEntity>().HasIndex(a => a.AnalysisDate);
    }
}

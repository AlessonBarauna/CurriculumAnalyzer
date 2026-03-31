using CurriculumAnalyzer.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurriculumAnalyzer.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<CurriculumEntity> Curriculums { get; set; }
    public DbSet<AnalysisEntity> Analyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<CurriculumEntity>()
            .HasOne(c => c.User)
            .WithMany(u => u.Curriculums)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AnalysisEntity>()
            .HasOne(a => a.Curriculum)
            .WithMany(c => c.Analyses)
            .HasForeignKey(a => a.CurriculumId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CurriculumEntity>().HasIndex(c => c.UploadedAt);
        modelBuilder.Entity<AnalysisEntity>().HasIndex(a => a.AnalysisDate);
    }
}

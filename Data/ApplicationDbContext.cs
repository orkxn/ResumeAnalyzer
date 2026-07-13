using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Analysis> Analyses { get; set; }
    public DbSet<ComparisonSession> ComparisonSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Resume -> Analysis (Bire-Bir)
        builder.Entity<Resume>()
            .HasOne(r => r.Analysis)
            .WithOne(a => a.Resume)
            .HasForeignKey<Analysis>(a => a.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Resumes (Bire-Çok)
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Resumes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> ComparisonSessions (Bire-Çok)
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.ComparisonSessions)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Not: List<string> ve List<int> alanları modern EF Core tarafından 
        // arka planda otomatik olarak algılanıp JSON olarak saklanacaktır.
    }
}
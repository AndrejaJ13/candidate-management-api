using Microsoft.EntityFrameworkCore;
using Zadatak.Models;

namespace Zadatak.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Candidate> Candidates => Set<Candidate>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(candidate => candidate.Id);

            entity.Property(candidate => candidate.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(candidate => candidate.ContactNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(candidate => candidate.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.Property(candidate => candidate.NormalizedEmail)
                .HasMaxLength(254)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.HasIndex(candidate => candidate.NormalizedEmail)
                .IsUnique();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(skill => skill.Id);

            entity.Property(skill => skill.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(skill => skill.NormalizedName)
                .HasMaxLength(100)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.HasIndex(skill => skill.NormalizedName)
                .IsUnique();
        });

        modelBuilder.Entity<CandidateSkill>(entity =>
        {
            entity.HasKey(candidateSkill => new
            {
                candidateSkill.CandidateId,
                candidateSkill.SkillId
            });

            entity.HasOne(candidateSkill => candidateSkill.Candidate)
                .WithMany(candidate => candidate.CandidateSkills)
                .HasForeignKey(candidateSkill => candidateSkill.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(candidateSkill => candidateSkill.Skill)
                .WithMany(skill => skill.CandidateSkills)
                .HasForeignKey(candidateSkill => candidateSkill.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>().HasData(
            new Candidate
            {
                Id = 1,
                FullName = "Ana Markovic",
                DateOfBirth = new DateOnly(1996, 4, 18),
                ContactNumber = "+38164111222",
                Email = "ana.markovic@example.com",
                NormalizedEmail = "ANA.MARKOVIC@EXAMPLE.COM"
            },
            new Candidate
            {
                Id = 2,
                FullName = "Petar Jovanovic",
                DateOfBirth = new DateOnly(1993, 9, 7),
                ContactNumber = "+38164222333",
                Email = "petar.jovanovic@example.com",
                NormalizedEmail = "PETAR.JOVANOVIC@EXAMPLE.COM"
            },
            new Candidate
            {
                Id = 3,
                FullName = "Milica Nikolic",
                DateOfBirth = new DateOnly(1999, 12, 2),
                ContactNumber = "+38164333444",
                Email = "milica.nikolic@example.com",
                NormalizedEmail = "MILICA.NIKOLIC@EXAMPLE.COM"
            });

        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, Name = "C# Programming", NormalizedName = "C# PROGRAMMING" },
            new Skill { Id = 2, Name = "Java Programming", NormalizedName = "JAVA PROGRAMMING" },
            new Skill { Id = 3, Name = "Database Design", NormalizedName = "DATABASE DESIGN" },
            new Skill { Id = 4, Name = "English", NormalizedName = "ENGLISH" },
            new Skill { Id = 5, Name = "German Language", NormalizedName = "GERMAN LANGUAGE" });

        modelBuilder.Entity<CandidateSkill>().HasData(
            new CandidateSkill { CandidateId = 1, SkillId = 1 },
            new CandidateSkill { CandidateId = 1, SkillId = 3 },
            new CandidateSkill { CandidateId = 1, SkillId = 4 },
            new CandidateSkill { CandidateId = 2, SkillId = 2 },
            new CandidateSkill { CandidateId = 2, SkillId = 3 },
            new CandidateSkill { CandidateId = 3, SkillId = 1 },
            new CandidateSkill { CandidateId = 3, SkillId = 5 });
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Mud_Q_Sass.API.Models;

namespace Mud_Q_Sass.API.Data;

public partial class SASDbContext : DbContext
{
    public SASDbContext()
    {
    }

    public SASDbContext(DbContextOptions<SASDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppUsers__3214EC07697A9D21");

            entity.HasIndex(e => e.Username, "UQ__AppUsers__536C85E454923E9A").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__AppUsers__A9D105341E67B5E9").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(64);
            entity.Property(e => e.PasswordSalt).HasMaxLength(32);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasMany(d => d.Branches).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserBranch",
                    r => r.HasOne<Branch>().WithMany()
                        .HasForeignKey("BranchId")
                        .HasConstraintName("FK_UserBranches_Branches"),
                    l => l.HasOne<AppUser>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserBranches_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "BranchId").HasName("PK__UserBran__5D9E4EB0B54017FD");
                        j.ToTable("UserBranches");
                    });

            entity.HasMany(d => d.Companies).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserCompany",
                    r => r.HasOne<Company>().WithMany()
                        .HasForeignKey("CompanyId")
                        .HasConstraintName("FK_UserCompanies_Companies"),
                    l => l.HasOne<AppUser>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserCompanies_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "CompanyId").HasName("PK__UserComp__C551BD86DD65C5DE");
                        j.ToTable("UserCompanies");
                    });
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branches__3214EC073ADED4BB");

            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.WorkingHours).HasMaxLength(100);

            entity.HasOne(d => d.Company).WithMany(p => p.Branches)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Branches_Companies");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Companie__3214EC078B413B1E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LogoPath).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC076EA857A2");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F67EA30205").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07CAB32BF5");

            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.HasIndex(e => e.UserId, "IX_UserRoles_UserId");

            entity.HasOne(d => d.Branch).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_UserRoles_Branches");

            entity.HasOne(d => d.Company).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_UserRoles_Companies");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);

            entity.HasOne(d => d.User)
                  .WithMany(p => p.RefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

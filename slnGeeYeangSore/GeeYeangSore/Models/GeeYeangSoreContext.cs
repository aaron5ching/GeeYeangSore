using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GeeYeangSore.Models;

public partial class GeeYeangSoreContext : DbContext
{
    public GeeYeangSoreContext()
    {
    }

    public GeeYeangSoreContext(DbContextOptions<GeeYeangSoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HAbout> HAbouts { get; set; }

    public virtual DbSet<HAudit> HAudits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=26.232.199.225;Initial Catalog=GeeYeangSore;User ID=admin02;Password=admin02;Encrypt=False;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HAbout>(entity =>
        {
            entity.ToTable("h_About");

            entity.Property(e => e.HAboutId).HasColumnName("h_About_Id");
            entity.Property(e => e.HContent).HasColumnName("h_Content");
            entity.Property(e => e.HCreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_CreatedAt");
            entity.Property(e => e.HTitle).HasColumnName("h_Title");
            entity.Property(e => e.HUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_UpdatedAt");
        });

        modelBuilder.Entity<HAudit>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("h_Audit");

            entity.Property(e => e.HAuditId)
                .ValueGeneratedOnAdd()
                .HasColumnName("h_Audit_Id");
            entity.Property(e => e.HBankAccount)
                .HasMaxLength(100)
                .HasColumnName("h_BankAccount");
            entity.Property(e => e.HBankName)
                .HasMaxLength(100)
                .HasColumnName("h_BankName");
            entity.Property(e => e.HIdCardBackPath).HasColumnName("h_IdCardBackPath");
            entity.Property(e => e.HIdCardFrontPath).HasColumnName("h_IdCardFrontPath");
            entity.Property(e => e.HReviewNote)
                .HasMaxLength(500)
                .HasColumnName("h_ReviewNote");
            entity.Property(e => e.HReviewedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_ReviewedAt");
            entity.Property(e => e.HStatus)
                .HasMaxLength(100)
                .HasColumnName("h_Status");
            entity.Property(e => e.HSubmittedAt)
                .HasColumnType("datetime")
                .HasColumnName("h_SubmittedAt");
            entity.Property(e => e.HTenantId).HasColumnName("h_Tenant_Id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

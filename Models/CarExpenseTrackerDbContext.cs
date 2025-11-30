using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CapExpenseTracker.Models;

public partial class CarExpenseTrackerDbContext : DbContext
{
    public CarExpenseTrackerDbContext()
    {
    }

    public CarExpenseTrackerDbContext(DbContextOptions<CarExpenseTrackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Expense> Expenses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=CarExpenseTrackerDB;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id); 


            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.CheckFile)
                .HasColumnType("character varying")
                .HasMaxLength(500);

            entity.Property(e => e.Comment)
                .HasColumnType("character varying")
                .HasMaxLength(1000);

            entity.Property(e => e.Periodicity)
                .HasColumnType("character varying")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.TypeExpense)
                .HasColumnType("character varying")
                .HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
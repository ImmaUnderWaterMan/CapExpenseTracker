using CapExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CapExpenseTracker.Data
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // БД будет в папке AppData пользователя
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CarExpenseTracker"
                );

                // Создаем папку если не существует
                Directory.CreateDirectory(appDataPath);

                string dbPath = Path.Combine(appDataPath, "CarExpenseTracker.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CheckFile)
                    .HasColumnType("TEXT")
                    .HasMaxLength(500);

                entity.Property(e => e.Comment)
                    .HasColumnType("TEXT")
                    .HasMaxLength(1000);

                entity.Property(e => e.Periodicity)
                    .HasColumnType("TEXT")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TypeExpense)
                    .HasColumnType("TEXT")
                    .HasMaxLength(100);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
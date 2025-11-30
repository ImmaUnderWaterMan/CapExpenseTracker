using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CapExpenseTracker.Models
{
    public partial class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        public string? TypeExpense { get; set; }

        public int Amount { get; set; }

        public int? CurrentRun { get; set; }

        public string? Comment { get; set; }

        public string? CheckFile { get; set; }

        public string Periodicity { get; set; } = null!;
    }
}
using CapExpenseTracker.Data;
using CapExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapExpenseTracker.Services
{
    public class SqliteDataService : IDataService
    {
        private readonly SqliteDbContext _context;

        public SqliteDataService()
        {
            _context = new SqliteDbContext();
            _context.Database.EnsureCreated();
        }

        // Все методы IDataService должны быть реализованы
        public async Task<List<Expense>> GetExpensesAsync()
        {
            return await _context.Expenses
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Expense> GetExpenseAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddExpenseAsync(Expense expense)
        {
            expense.Id = 0;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateExpenseAsync(Expense expense)
        {
            var existingExpense = await _context.Expenses.FindAsync(expense.Id);
            if (existingExpense != null)
            {
                _context.Entry(existingExpense).CurrentValues.SetValues(expense);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"Expense with ID {expense.Id} not found");
            }
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Expense>> GetExpensesByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.Date.ToDateTime(TimeOnly.MinValue) >= startDate &&
                           e.Date.ToDateTime(TimeOnly.MinValue) <= endDate)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<List<string>> GetExpenseTypesAsync()
        {
            return await _context.Expenses
                .Where(e => !string.IsNullOrEmpty(e.TypeExpense))
                .Select(e => e.TypeExpense)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
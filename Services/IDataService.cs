using CapExpenseTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapExpenseTracker.Services
{
    public interface IDataService
    {
        Task<List<Expense>> GetExpensesAsync();
        Task<Expense> GetExpenseAsync(int id);
        Task AddExpenseAsync(Expense expense);
        Task UpdateExpenseAsync(Expense expense);
        Task DeleteExpenseAsync(int id);
        Task<List<Expense>> GetExpensesByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<List<string>> GetExpenseTypesAsync();
    }
}
using CapExpenseTracker.Models;
using CapExpenseTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CapExpenseTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private ObservableCollection<Expense> _expenses;
        private Expense _selectedExpense;
        private string _selectedPeriod = "Все";
        private string _selectedExpenseType = "Все типы";
        private decimal _totalAmount;
        private decimal _averageMonthly;
        private string _sortColumn = "Date";
        private ListSortDirection _sortDirection = ListSortDirection.Descending;
        private ICollectionView _expensesView;

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _expenses = new ObservableCollection<Expense>();
            _expensesView = CollectionViewSource.GetDefaultView(_expenses);

            LoadCommands();
            InitializeSorting();
            _ = LoadDataAsync();
        }

        public ICollectionView ExpensesView => _expensesView;

        public Expense SelectedExpense
        {
            get => _selectedExpense;
            set => SetProperty(ref _selectedExpense, value);
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                SetProperty(ref _selectedPeriod, value);
                ApplyFilters();
            }
        }

        public string SelectedExpenseType
        {
            get => _selectedExpenseType;
            set
            {
                SetProperty(ref _selectedExpenseType, value);
                ApplyFilters();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public decimal AverageMonthly
        {
            get => _averageMonthly;
            set => SetProperty(ref _averageMonthly, value);
        }

        public ObservableCollection<CategoryStat> CategoryStats { get; } = new ObservableCollection<CategoryStat>();

        public List<string> Periods { get; } = new List<string> { "Неделя", "Месяц", "Квартал", "Год", "Все" };
        public ObservableCollection<string> ExpenseTypes { get; } = new ObservableCollection<string>();


        public ICommand AddExpenseCommand { get; private set; }
        public ICommand EditExpenseCommand { get; private set; }
        public ICommand DeleteExpenseCommand { get; private set; }
        public ICommand ShowStatisticsCommand { get; private set; }
        public ICommand SortByDateCommand { get; private set; }
        public ICommand SortByAmountCommand { get; private set; }
        public ICommand SortByMileageCommand { get; private set; }

        private void LoadCommands()
        {
            AddExpenseCommand = new RelayCommand(_ => AddExpense());
            EditExpenseCommand = new RelayCommand(_ => EditExpense(), _ => SelectedExpense != null);
            DeleteExpenseCommand = new RelayCommand(_ => DeleteExpense(), _ => SelectedExpense != null);
            ShowStatisticsCommand = new RelayCommand(_ => ShowStatistics());
            SortByDateCommand = new RelayCommand(_ => SortBy("Date"));
            SortByAmountCommand = new RelayCommand(_ => SortBy("Amount"));
            SortByMileageCommand = new RelayCommand(_ => SortBy("CurrentRun"));
        }

        private void InitializeSorting()
        {
            _expensesView.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var expenses = await _dataService.GetExpensesAsync();
                var expenseTypes = await _dataService.GetExpenseTypesAsync();


                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Expenses.Clear();
                    foreach (var expense in expenses)
                    {
                        Expenses.Add(expense);
                    }

                    ExpenseTypes.Clear();
                    ExpenseTypes.Add("Все типы");
                    foreach (var type in expenseTypes)
                    {
                        ExpenseTypes.Add(type);
                    }

                    CalculateStatistics();
                    UpdateCategoryStats();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private ObservableCollection<Expense> Expenses
        {
            get => _expenses;
            set
            {
                _expenses = value;
                _expensesView = CollectionViewSource.GetDefaultView(_expenses);
                InitializeSorting();
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            _expensesView.Filter = obj =>
            {
                if (obj is not Expense expense) return false;


                var today = DateTime.Today;
                var expenseDate = expense.Date.ToDateTime(TimeOnly.MinValue);
                bool periodFilter = SelectedPeriod switch
                {
                    "Неделя" => expenseDate >= today.AddDays(-7),
                    "Месяц" => expenseDate >= today.AddMonths(-1),
                    "Квартал" => expenseDate >= today.AddMonths(-3),
                    "Год" => expenseDate >= today.AddYears(-1),
                    _ => true
                };


                bool typeFilter = SelectedExpenseType == "Все типы" || expense.TypeExpense == SelectedExpenseType;

                return periodFilter && typeFilter;
            };

            CalculateStatistics();
            UpdateCategoryStats();
        }

        private void CalculateStatistics()
        {
            var filteredExpenses = _expensesView.Cast<Expense>().ToList();
            TotalAmount = filteredExpenses.Sum(e => e.Amount);

            var monthlyExpenses = filteredExpenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => (decimal)g.Sum(e => e.Amount))
                .ToList();

            AverageMonthly = monthlyExpenses.Any() ? monthlyExpenses.Average() : 0;
        }

        private void UpdateCategoryStats()
        {
            var filteredExpenses = _expensesView.Cast<Expense>().ToList();
            var categoryGroups = filteredExpenses
                .GroupBy(e => e.TypeExpense ?? "Без категории")
                .Select(g => new CategoryStat
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Percentage = TotalAmount > 0 ? (decimal)g.Sum(e => e.Amount) / TotalAmount * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            CategoryStats.Clear();
            foreach (var stat in categoryGroups)
            {
                CategoryStats.Add(stat);
            }
        }

        private void SortBy(string columnName)
        {

            if (_sortColumn == columnName)
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _sortColumn = columnName;
                _sortDirection = ListSortDirection.Ascending;
            }

            _expensesView.SortDescriptions.Clear();
            _expensesView.SortDescriptions.Add(new SortDescription(columnName, _sortDirection));
        }

        private void AddExpense()
        {
            try
            {
                var addEditWindow = new AddEditExpenseWindow();
                if (addEditWindow.ShowDialog() == true)
                {
                    _ = LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditExpense()
        {
            if (SelectedExpense == null) return;

            try
            {
                var addEditWindow = new AddEditExpenseWindow(SelectedExpense);
                if (addEditWindow.ShowDialog() == true)
                {
                    _ = LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteExpense()
        {
            if (SelectedExpense == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить расход от {SelectedExpense.Date:dd.MM.yyyy} на сумму {SelectedExpense.Amount} руб.?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _dataService.DeleteExpenseAsync(SelectedExpense.Id);


                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Expenses.Remove(SelectedExpense);
                        CalculateStatistics();
                        UpdateCategoryStats();
                    });

                    MessageBox.Show("Расход успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ShowStatistics()
        {
            try
            {
                var statisticsWindow = new StatisticsWindow();
                statisticsWindow.Owner = Application.Current.MainWindow;


                statisticsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class CategoryStat
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
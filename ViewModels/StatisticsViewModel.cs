using CapExpenseTracker.Models;
using CapExpenseTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CapExpenseTracker.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedPeriod = "Месяц";
        private bool _isLoading;

        public StatisticsViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _startDate = DateTime.Today.AddMonths(-1);
            _endDate = DateTime.Today;

            LoadCommands();

        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    ApplyPeriodFilter();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<Expense> Expenses { get; } = new ObservableCollection<Expense>();
        public ObservableCollection<StatisticsCategoryStat> CategoryStats { get; } = new ObservableCollection<StatisticsCategoryStat>();
        public ObservableCollection<MonthlyStat> MonthlyStats { get; } = new ObservableCollection<MonthlyStat>();

        public decimal TotalAmount { get; private set; }
        public decimal CostPerKm { get; private set; }
        public decimal YearlyForecast { get; private set; }
        public string ComparisonText { get; private set; } = "Нет данных для сравнения";
        public string ForecastDetails { get; private set; } = "На основе периодических платежей";

        public List<string> Periods { get; } = new List<string>
        {
            "Неделя", "Месяц", "Квартал", "Год", "Произвольный"
        };


        public ICommand ApplyFilterCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand LoadedCommand { get; private set; }

        private void LoadCommands()
        {
            ApplyFilterCommand = new RelayCommand(async _ => await LoadDataAsync());
            ExportCommand = new RelayCommand(_ => ExportData());
            LoadedCommand = new RelayCommand(async _ => await LoadDataAsync());
        }

        private void ApplyPeriodFilter()
        {
            var today = DateTime.Today;
            switch (SelectedPeriod)
            {
                case "Неделя":
                    StartDate = today.AddDays(-7);
                    EndDate = today;
                    break;
                case "Месяц":
                    StartDate = today.AddMonths(-1);
                    EndDate = today;
                    break;
                case "Квартал":
                    StartDate = today.AddMonths(-3);
                    EndDate = today;
                    break;
                case "Год":
                    StartDate = today.AddYears(-1);
                    EndDate = today;
                    break;
                case "Произвольный":

                    break;
            }
        }

        public async Task LoadDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;


                var expenses = await Task.Run(() =>
                    _dataService.GetExpensesByPeriodAsync(StartDate, EndDate).Result
                );


                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Expenses.Clear();
                    foreach (var expense in expenses)
                    {
                        Expenses.Add(expense);
                    }

                    CalculateStatistics();
                    UpdateCategoryStats();
                    UpdateMonthlyStats();
                    CalculatePeriodComparison();
                    CalculateYearlyForecast();
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
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateStatistics()
        {
            TotalAmount = Expenses.Sum(e => e.Amount);


            var mileageExpenses = Expenses.Where(e => e.CurrentRun.HasValue && e.CurrentRun > 0).ToList();
            var totalMileage = mileageExpenses.Sum(e => e.CurrentRun.Value);
            var totalMileageAmount = mileageExpenses.Sum(e => e.Amount);

            CostPerKm = totalMileage > 0 ? totalMileageAmount / totalMileage : 0;

            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(CostPerKm));
        }

        private void UpdateCategoryStats()
        {
            var categoryGroups = Expenses
                .GroupBy(e => e.TypeExpense ?? "Без категории")
                .Select(g => new StatisticsCategoryStat
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count(),
                    AverageAmount = g.Count() > 0 ? g.Sum(e => e.Amount) / g.Count() : 0,
                    Percentage = TotalAmount > 0 ? (g.Sum(e => e.Amount) / TotalAmount) * 100 : 0,
                    CostPerKm = CalculateCategoryCostPerKm(g.ToList())
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            CategoryStats.Clear();
            foreach (var stat in categoryGroups)
            {
                CategoryStats.Add(stat);
            }
        }

        private decimal CalculateCategoryCostPerKm(List<Expense> expenses)
        {
            var mileageExpenses = expenses.Where(e => e.CurrentRun.HasValue && e.CurrentRun > 0).ToList();
            var totalMileage = mileageExpenses.Sum(e => e.CurrentRun.Value);
            var totalAmount = mileageExpenses.Sum(e => e.Amount);

            return totalMileage > 0 ? totalAmount / totalMileage : 0;
        }

        private void UpdateMonthlyStats()
        {
            var monthlyGroups = Expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new MonthlyStat
                {
                    Period = $"{g.Key.Month:00}.{g.Key.Year}",
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderBy(m => m.Period)
                .ToList();


            var maxAmount = monthlyGroups.Any() ? monthlyGroups.Max(m => m.Amount) : 1;
            foreach (var stat in monthlyGroups)
            {
                stat.MaxAmount = maxAmount;
            }

            MonthlyStats.Clear();
            foreach (var stat in monthlyGroups)
            {
                MonthlyStats.Add(stat);
            }
        }

        private void CalculatePeriodComparison()
        {
            try
            {
                var currentPeriodExpenses = Expenses.Sum(e => e.Amount);
                var previousPeriodStart = GetPreviousPeriodStart();
                var previousPeriodEnd = StartDate.AddDays(-1);


                var previousExpenses = Task.Run(() =>
                    _dataService.GetExpensesByPeriodAsync(previousPeriodStart, previousPeriodEnd).Result
                ).Result;

                var previousAmount = previousExpenses.Sum(e => e.Amount);

                if (previousAmount > 0)
                {
                    var difference = currentPeriodExpenses - previousAmount;
                    var percentage = (difference / previousAmount) * 100;

                    var trend = difference >= 0 ? "увеличились" : "снизились";
                    var absDifference = Math.Abs(difference);
                    var absPercentage = Math.Abs(percentage);

                    ComparisonText = $"Расходы {trend} на {absDifference:0} руб. ({absPercentage:0.0}%) " +
                                   $"по сравнению с предыдущим периодом";
                }
                else
                {
                    ComparisonText = "Нет данных за предыдущий период для сравнения";
                }
            }
            catch (Exception ex)
            {
                ComparisonText = "Не удалось загрузить данные за предыдущий период";
                System.Diagnostics.Debug.WriteLine($"Ошибка сравнения периодов: {ex.Message}");
            }

            OnPropertyChanged(nameof(ComparisonText));
        }

        private DateTime GetPreviousPeriodStart()
        {
            var periodLength = EndDate - StartDate;
            return StartDate - periodLength;
        }

        private void CalculateYearlyForecast()
        {

            var periodicExpenses = Expenses.Where(e => e.Periodicity != "Разовый").ToList();
            var yearlyForecast = 0m;

            foreach (var expense in periodicExpenses)
            {
                yearlyForecast += expense.Periodicity switch
                {
                    "Ежемесячный" => expense.Amount * 12,
                    "Ежеквартальный" => expense.Amount * 4,
                    "Ежегодный" => expense.Amount,
                    _ => 0
                };
            }


            var oneTimeExpenses = Expenses.Where(e => e.Periodicity == "Разовый").ToList();

            if (oneTimeExpenses.Any())
            {
                var totalAmount = oneTimeExpenses.Sum(e => e.Amount);
                var count = oneTimeExpenses.Count();
                var averageMonthlyOneTime = totalAmount / count;

                yearlyForecast += averageMonthlyOneTime * 12;
            }

            YearlyForecast = yearlyForecast;
            ForecastDetails = $"На основе {periodicExpenses.Count()} периодических платежей " +
                            $"и {oneTimeExpenses.Count()} разовых расходов";

            OnPropertyChanged(nameof(YearlyForecast));
            OnPropertyChanged(nameof(ForecastDetails));
        }

        private void ExportData()
        {
            MessageBox.Show("Функция экспорта будет реализована в будущем", "Экспорт",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class StatisticsCategoryStat
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal CostPerKm { get; set; }
    }

    public class MonthlyStat
    {
        public string Period { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal MaxAmount { get; set; }
    }
}
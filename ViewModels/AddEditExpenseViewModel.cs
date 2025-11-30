using CapExpenseTracker.Models;
using CapExpenseTracker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CapExpenseTracker.ViewModels
{
    public class AddEditExpenseViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IDataService _dataService;
        private Expense _expense;
        private bool _isEditMode;

        public AddEditExpenseViewModel(IDataService dataService, Expense expense = null)
        {
            _dataService = dataService;
            _isEditMode = expense != null;
            _expense = expense ?? new Expense
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                Amount = 0,
                Periodicity = "Разовый",
                TypeExpense = "Топливо" 
            };


            LoadDefaultExpenseTypes();

            LoadCommands();
            _ = LoadExpenseTypesFromDatabaseAsync();
        }

        public DateOnly Date
        {
            get => _expense.Date;
            set
            {
                _expense.Date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateDateTime));
            }
        }

        public DateTime DateDateTime
        {
            get => _expense.Date.ToDateTime(TimeOnly.MinValue);
            set
            {
                Date = DateOnly.FromDateTime(value);
                OnPropertyChanged(nameof(Date));
            }
        }

        public string TypeExpense
        {
            get => _expense.TypeExpense;
            set
            {
                _expense.TypeExpense = value;
                OnPropertyChanged();
            }
        }

        public int Amount
        {
            get => (int)_expense.Amount;
            set
            {
                _expense.Amount = value;
                OnPropertyChanged();
            }
        }

        public int? CurrentRun
        {
            get => _expense.CurrentRun;
            set
            {
                _expense.CurrentRun = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get => _expense.Comment;
            set
            {
                _expense.Comment = value;
                OnPropertyChanged();
            }
        }

        public string Periodicity
        {
            get => _expense.Periodicity;
            set
            {
                _expense.Periodicity = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ExpenseTypes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> PeriodicityTypes { get; } = new ObservableCollection<string>
        {
            "Разовый",
            "Ежемесячный",
            "Ежеквартальный",
            "Ежегодный"
        };

        public string Title => _isEditMode ? "Редактирование расхода" : "Добавление расхода";


        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public bool CanSave => !HasErrors;

        private void LoadCommands()
        {
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void LoadDefaultExpenseTypes()
        {
            ExpenseTypes.Clear();
            var defaultTypes = new List<string>
            {
                "Топливо",
                "Техническое обслуживание",
                "Страховка",
                "Ремонт",
                "Шины",
                "Мойка",
                "Парковка",
                "Штрафы",
                "Прочее"
            };

            foreach (var type in defaultTypes)
            {
                ExpenseTypes.Add(type);
            }


            if (!string.IsNullOrEmpty(TypeExpense) && !ExpenseTypes.Contains(TypeExpense))
            {
                ExpenseTypes.Add(TypeExpense);
            }
        }

        private async Task LoadExpenseTypesFromDatabaseAsync()
        {
            try
            {
                var expenseTypesFromDb = await _dataService.GetExpenseTypesAsync();


                await Application.Current.Dispatcher.InvokeAsync(() =>
                {

                    foreach (var type in expenseTypesFromDb)
                    {
                        if (!string.IsNullOrEmpty(type) && !ExpenseTypes.Contains(type))
                        {
                            ExpenseTypes.Add(type);
                        }
                    }


                    var sortedList = ExpenseTypes.OrderBy(t => t).ToList();
                    ExpenseTypes.Clear();
                    foreach (var type in sortedList)
                    {
                        ExpenseTypes.Add(type);
                    }


                    if (_isEditMode && !string.IsNullOrEmpty(TypeExpense) && !ExpenseTypes.Contains(TypeExpense))
                    {
                        ExpenseTypes.Add(TypeExpense);
                    }
                });
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки типов из БД: {ex.Message}");
            }
        }

        private void Save()
        {
            try
            {

                if (HasErrors)
                {
                    MessageBox.Show("Исправьте ошибки в форме перед сохранением.", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    _dataService.UpdateExpenseAsync(_expense).Wait();
                    MessageBox.Show("Расход успешно обновлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _dataService.AddExpenseAsync(_expense).Wait();
                    MessageBox.Show("Расход успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        #region IDataErrorInfo Implementation

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(DateDateTime):
                        if (DateDateTime > DateTime.Today)
                            return "Дата не может быть будущей";
                        break;

                    case nameof(Amount):
                        if (Amount <= 0)
                            return "Сумма должна быть положительной";
                        break;

                    case nameof(CurrentRun):
                        if (CurrentRun.HasValue && CurrentRun < 0)
                            return "Пробег не может быть отрицательным";
                        break;

                    case nameof(TypeExpense):
                        if (string.IsNullOrWhiteSpace(TypeExpense))
                            return "Выберите тип расхода";
                        break;
                }

                return null;
            }
        }

        public bool HasErrors
        {
            get
            {
                return !string.IsNullOrEmpty(this[nameof(DateDateTime)]) ||
                       !string.IsNullOrEmpty(this[nameof(Amount)]) ||
                       !string.IsNullOrEmpty(this[nameof(CurrentRun)]) ||
                       !string.IsNullOrEmpty(this[nameof(TypeExpense)]);
            }
        }

        #endregion
    }
}
using CapExpenseTracker.Models;
using CapExpenseTracker.Services;
using CapExpenseTracker.ViewModels;
using System.Windows;

namespace CapExpenseTracker
{
    public partial class AddEditExpenseWindow : Window
    {
        public AddEditExpenseWindow()
        {
            InitializeComponent();

            var dataService = new DataService();
            var viewModel = new AddEditExpenseViewModel(dataService);
            DataContext = viewModel;
        }

        public AddEditExpenseWindow(Expense expense)
        {
            InitializeComponent();

            var dataService = new DataService();
            var viewModel = new AddEditExpenseViewModel(dataService, expense);
            DataContext = viewModel;
        }
    }
}
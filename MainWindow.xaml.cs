using CapExpenseTracker.Services;
using CapExpenseTracker.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CapExpenseTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dataService = new DataService();
            var viewModel = new MainViewModel(dataService);
            DataContext = viewModel;
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {

            e.Handled = false; 
        }
    }
}
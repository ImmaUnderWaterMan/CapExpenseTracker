using CapExpenseTracker.Services;
using CapExpenseTracker.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace CapExpenseTracker
{
    public partial class StatisticsWindow : Window
    {
        private readonly StatisticsViewModel _viewModel;

        public StatisticsWindow()
        {
            InitializeComponent();

            var dataService = new DataService();
            _viewModel = new StatisticsViewModel(dataService);
            DataContext = _viewModel;


            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            await _viewModel.LoadDataAsync();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatisticsViewModel.IsLoading))
            {

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingOverlay.Visibility = _viewModel.IsLoading ? Visibility.Visible : Visibility.Collapsed;
                }), DispatcherPriority.Background);
            }
        }

        protected override void OnClosed(EventArgs e)
        {

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}
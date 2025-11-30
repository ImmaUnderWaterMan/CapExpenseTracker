using CapExpenseTracker.Models;
using CapExpenseTracker.Services;
using CapExpenseTracker.ViewModels;
using System;
using System.Windows;

namespace CapExpenseTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IDataService dataService;

            try
            {

                using var postgresContext = new CarExpenseTrackerDbContext();
                if (postgresContext.Database.CanConnect())
                {
                    dataService = new DataService(); 
                    Console.WriteLine("Using PostgreSQL database");
                }
                else
                {
                    throw new Exception("PostgreSQL not available");
                }
            }
            catch (Exception ex)
            {

                dataService = new SqliteDataService();
                Console.WriteLine($"Using SQLite database. Reason: {ex.Message}");
            }

            var mainViewModel = new MainViewModel(dataService);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
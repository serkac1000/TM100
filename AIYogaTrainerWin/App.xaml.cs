using System;
using System.Windows;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Load application settings on startup
            try
            {
                AppSettings.LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "AI Yoga Trainer", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                
                // Initialize default settings if loading fails
                AppSettings.InitializeDefaultSettings();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            // Save application settings on exit
            try
            {
                AppSettings.SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "AI Yoga Trainer", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

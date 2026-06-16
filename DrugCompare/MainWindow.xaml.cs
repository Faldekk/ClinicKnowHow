using DrugCompare.ViewModels;
using System.Windows;

namespace DrugCompare;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OpenHistory_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.HistoryView
        {
            DataContext = DataContext
        };

        ShowSimpleWindow("History", window);
    }

    private void OpenDatabaseStatus_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.DatabaseStatusView
        {
            DataContext = DataContext
        };

        ShowSimpleWindow("Database Status", window);
    }

    private void OpenDataManagement_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.DataManagementView
        {
            DataContext = DataContext
        };

        ShowSimpleWindow("Data Management", window);
    }

    private void OpenAuditLog_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.AuditLogView
        {
            DataContext = DataContext
        };

        ShowSimpleWindow("Audit Log", window);
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.SettingsView
        {
            DataContext = DataContext
        };

        ShowSimpleWindow("Settings", window);
    }

    private void OpenAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "MedCompare\n\nLocal medical reference and interaction-checking prototype.\n\nThis application does not replace physician or pharmacist judgment.",
            "About MedCompare",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ShowSimpleWindow(string title, object content)
    {
        var window = new Window
        {
            Title = title,
            Content = content,
            Owner = this,
            Width = 900,
            Height = 650,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        window.ShowDialog();
    }
}

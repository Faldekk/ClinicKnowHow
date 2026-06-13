using System.Windows;
using System.Windows.Controls;
using DrugCompare.ViewModels;
using DrugCompare.Views;

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
        OpenToolWindow("History", new HistoryView());
    }

    private void OpenDatabaseStatus_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Database Status", new DatabaseStatusView());
    }

    private void OpenDataManagement_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Data Management", new DataManagementView());
    }

    private void OpenAuditLog_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Audit Log", new AuditLogView());
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Settings", new SettingsView());
    }

    private void OpenAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "MedCompare\n\nLocal educational clinical decision-support prototype.\n\nThis application does not diagnose, prescribe, recommend treatment, or replace clinical judgment.",
            "About MedCompare",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OpenToolWindow(string title, UserControl view)
    {
        view.DataContext = DataContext;

        var window = new ToolWindow(title, view, DataContext!)
        {
            Owner = this
        };

        window.Show();
    }
    private void OpenDrugExplorer_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Drug Explorer", new DrugExplorerView());
    }
    private void OpenPolishDrugRegistry_Click(object sender, RoutedEventArgs e)
    {
        OpenToolWindow("Polish Drug Registry", new PolishDrugRegistryView());
    }
}
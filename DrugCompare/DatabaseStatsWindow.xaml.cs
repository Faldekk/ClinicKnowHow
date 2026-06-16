using DrugCompare.ViewModels;
using System.Windows;

namespace DrugCompare;

public partial class DatabaseStatusWindow : Window
{
    public DatabaseStatusWindow(DatabaseStatusViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

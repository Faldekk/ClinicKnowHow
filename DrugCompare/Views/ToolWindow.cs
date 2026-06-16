using System.Windows;
using System.Windows.Controls;

namespace DrugCompare.Views;

public class ToolWindow : Window
{
    public ToolWindow(string title, UserControl content, object dataContext)
    {
        Title = title;
        Width = 1050;
        Height = 680;
        MinWidth = 850;
        MinHeight = 520;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.CanResize;

        content.DataContext = dataContext;

        Content = new Border
        {
            Background = System.Windows.Media.Brushes.White,
            Padding = new Thickness(8),
            Child = content
        };
    }
}

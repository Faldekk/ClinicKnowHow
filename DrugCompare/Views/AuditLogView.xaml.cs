using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCompare.Views;

public partial class AuditLogView : UserControl
{
    public AuditLogView()
    {
        InitializeComponent();
        IsVisibleChanged += AuditLogView_IsVisibleChanged;
    }

    private async void AuditLogView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true)
        {
            return;
        }

        if (DataContext is null)
        {
            return;
        }

        if (DataContext.GetType().GetProperty("LoadAuditLogsCommand")?.GetValue(DataContext) is not ICommand command)
        {
            return;
        }

        if (!command.CanExecute(null))
        {
            return;
        }

        command.Execute(null);

        await Task.CompletedTask;
    }
}

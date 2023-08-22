using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaMail;

public partial class AboutBox : Window
{
    public bool CancelClosing = true;

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (CancelClosing)
        {
            e.Cancel = true;
        }

        Hide();
    }

    private void Ok_OnClick(object? sender, RoutedEventArgs e)
    {
        Hide();
    }
}
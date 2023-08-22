using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaMail;

public partial class AskPassword : Window
{
    public AskPassword()
    {
        InitializeComponent();
    }
    
    private void Ok_OnClick(object? sender, RoutedEventArgs e)
    {
        Reference.Password = Password.Text;

        Close();
    }
}
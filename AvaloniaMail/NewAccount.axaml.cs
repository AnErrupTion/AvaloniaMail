using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaMail;

public partial class NewAccount : Window
{
    private void Ok_OnClick(object? sender, RoutedEventArgs e)
    {
        Reference.ImapAddress = ImapAddress.Text;
        Reference.ImapPort = string.IsNullOrEmpty(ImapPort.Text) ? 0 : int.Parse(ImapPort.Text);
        Reference.Address = Address.Text;
        Reference.Password = Password.Text;

        Close();
    }
}
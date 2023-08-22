using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaMail;

public partial class MainWindow : Window
{
    private bool _cancelClosing = true, _gotFocus;

    private readonly AboutBox _aboutBox = new();
    private readonly Dictionary<string, AvaloniaList<MailMsg>> _messages = new();

    private void About_OnClick(object? sender, RoutedEventArgs e)
    {
        _aboutBox.Show();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_cancelClosing)
        {
            return;
        }

        e.Cancel = true;
        Exit();
    }

    private async void Setup_OnClick(object? sender, RoutedEventArgs e)
    {
        Reference.ImapAddress = string.Empty;
        Reference.Address = string.Empty;
        Reference.Password = string.Empty;
        
        await new NewAccount().ShowDialog(this);

        if (string.IsNullOrEmpty(Reference.ImapAddress)
            || string.IsNullOrEmpty(Reference.Address)
            || string.IsNullOrEmpty(Reference.Password)
           )
            return;

        var account = new MailAccount(Reference.ImapAddress, Reference.ImapPort, Reference.Address, Reference.Password);

        Accounts.Items.Add(account);

        await File.AppendAllLinesAsync("accounts.dat", account.ToArray());

        Load(account);
    }

    private void Exit_OnClick(object? sender, RoutedEventArgs e)
    {
        Exit();
    }

    private void Accounts_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is not MailAccount selectedAccount)
            throw new ArgumentNullException(nameof(selectedAccount));

        Load(selectedAccount);
    }

    private async void Window_OnFocus(object? sender, GotFocusEventArgs e)
    {
        if (_gotFocus || !File.Exists("accounts.dat"))
            return;

        var lines = await File.ReadAllLinesAsync("accounts.dat");
        var index = 0;

        while (index < lines.Length)
        {
            var imapAddress = lines[index++];
            var imapPort = int.Parse(lines[index++]);
            var address = lines[index++];

            await AskPassword(address);

            if (string.IsNullOrEmpty(Reference.Password))
                continue;

            Accounts.Items.Add(new MailAccount(imapAddress, imapPort, address, Reference.Password));
        }

        _gotFocus = true;
    }

    private void Mails_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (Mails.SelectedItem is not MailMsg mail)
            throw new ArgumentNullException(nameof(mail));

        var view = new BodyView
        {
            Title = $"{mail.Message.Subject} - {mail.Message.From}",
            View =
            {
                Text = string.IsNullOrEmpty(mail.Message.HtmlBody) ? mail.Message.TextBody : mail.Message.HtmlBody
            }
        };

        view.Show();
    }

    private Task AskPassword(string address)
    {
        Reference.Password = string.Empty;

        var info = new AskPassword();
        info.Message.Text = info.Message.Text?.Replace("{0}", address);

        return info.ShowDialog(this);
    }

    private async void Load(MailAccount account)
    {
        Mails.Items.Clear();

        await MailInterface.ConnectAsync(account);
        await MailInterface.UpdateMessagesAsync(account.Address);

        if (_messages.TryGetValue(account.Address, out var newMessages))
        {
            await MailInterface.CheckNewMessagesAsync(newMessages.Count);
        }

        var messages = new AvaloniaList<MailMsg>();
        foreach (var message in MailInterface.Messages)
            Mails.Items.Add(message);

        _messages.Add(account.Address, messages);

        Console.WriteLine("Done!");
    }

    private void Exit()
    {
        _aboutBox.CancelClosing = false;
        _aboutBox.Close();

        _cancelClosing = false;
        Close();
    }
}
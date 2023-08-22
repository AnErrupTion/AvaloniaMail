using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;

namespace AvaloniaMail;

public static class MailInterface
{
    public static readonly ConcurrentBag<MailMsg> Messages = new();

    private static readonly ImapClient ImapClient = new();
    private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = 64 };

    private static string _directory;
    private static int _length;

    public static Task ConnectAsync(MailAccount account)
        => Task.Factory.StartNew(acc => Connect(acc as MailAccount ?? throw new ArgumentNullException(nameof(acc))), account);

    public static Task UpdateMessagesAsync(string address)
        => Task.Factory.StartNew(addr => UpdateMessages(addr as string ?? throw new ArgumentNullException(nameof(addr))), address);

    public static Task CheckNewMessagesAsync(int currentCount)
        => Task.Factory.StartNew(count => CheckNewMessages(int.Parse(count as string ?? throw new ArgumentNullException(nameof(count)))), currentCount.ToString());

    public static void Connect(MailAccount account)
    {
        if (ImapClient.IsConnected)
        {
            Console.WriteLine("Disconnecting...");

            ImapClient.Disconnect(true);
        }

        Console.WriteLine("Connecting...");

        ImapClient.Connect(account.ImapAddress, account.ImapPort, SecureSocketOptions.SslOnConnect);

        Console.WriteLine("Authenticating...");

        ImapClient.Authenticate(account.Address, account.Password);
    }

    public static void CheckNewMessages(int currentCount)
    {
        Console.WriteLine("Checking for new messages...");

        if (ImapClient.Inbox.Count == currentCount)
            return;

        Console.WriteLine("Found new messages!");

        ForceUpdateMessages();
    }

    public static void UpdateMessages(string address)
    {
        if (!ImapClient.Inbox.IsOpen)
        {
            Console.WriteLine("Opening Inbox...");

            ImapClient.Inbox.Open(FolderAccess.ReadOnly);
        }

        Console.WriteLine("Getting messages...");

        Messages.Clear();

        _directory = address;

        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);

            ForceUpdateMessages();
        }
        else
        {
            var files = Directory.GetFiles(_directory);

            _length = files.Length;

            Parallel.ForEach(files, ParallelOptions, GetCached);
        }
    }

    private static void ForceUpdateMessages()
    {
        _length = ImapClient.Inbox.Count;

        Parallel.ForEach(ImapClient.Inbox, ParallelOptions, AddAndCache);
    }

    private static void AddAndCache(MimeMessage msg, ParallelLoopState state, long index)
    {
        Messages.Add(new(msg));

        using var stream = File.Create($"{_directory}/{index}.dat");
        msg.WriteTo(stream);

        Console.WriteLine($"Getting messages... ({index.ToString()}/{_length.ToString()})");
    }

    private static void GetCached(string file, ParallelLoopState state, long index)
    {
        var msg = MimeMessage.Load(file);

        if (msg is null)
            return;

        Messages.Add(new(msg));

        Console.WriteLine($"Getting messages... ({index.ToString()}/{_length.ToString()})");
    }
}
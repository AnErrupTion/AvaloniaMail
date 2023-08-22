using System.Collections.Generic;

namespace AvaloniaMail;

public sealed record MailAccount(string ImapAddress, int ImapPort, string Address, string Password = "")
{
    public override string ToString() => Address;

    public IEnumerable<string> ToArray()
    {
        return new[]
        {
            ImapAddress,
            ImapPort.ToString(),
            Address
        };
    }
}
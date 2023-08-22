using MimeKit;

namespace AvaloniaMail;

public sealed record MailMsg(MimeMessage Message)
{
    public override string ToString() => Message.Subject;
}
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ssdp_2600.Services;

public class IdentityEmailSender : IEmailSender
{
    private readonly IFluentEmail _email;

    public IdentityEmailSender(IFluentEmail email)
    {
        _email = email;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _email
            .To(email)
            .Subject(subject)
            .Body(htmlMessage, isHtml: true)
            .SendAsync();
    }
}
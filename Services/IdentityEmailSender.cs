using FluentEmail.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LuxRentals.Services;

public class IdentityEmailSender : IEmailSender
{
    private readonly IFluentEmailFactory _emailFactory;
    private readonly string _templatePath;

    public IdentityEmailSender(IFluentEmailFactory emailFactory, IWebHostEnvironment environment)
    {
        _emailFactory = emailFactory;
        _templatePath = Path.Combine(environment.ContentRootPath, "EmailTemplates", "IdentityEmail.cshtml");
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _emailFactory
            .Create()
            .To(email)
            .Subject(subject)
            .UsingTemplateFromFile(_templatePath, new IdentityEmailTemplateModel
            {
                Subject = subject,
                HtmlMessage = new HtmlString(htmlMessage)
            })
            .SendAsync();
    }
}
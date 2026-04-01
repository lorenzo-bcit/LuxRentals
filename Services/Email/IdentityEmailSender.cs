using FluentEmail.Core;
using LuxRentals.ViewModels.Email;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LuxRentals.Services.Email;

public class IdentityEmailSender : IEmailSender
{
    private const string TEMPLATE_PATH = "Views/Email/IdentityEmail.cshtml";

    private readonly IFluentEmailFactory _emailFactory;
    private readonly string _templatePath;

    public IdentityEmailSender(IFluentEmailFactory emailFactory, IWebHostEnvironment environment)
    {
        _emailFactory = emailFactory;
        _templatePath = Path.Combine(environment.ContentRootPath, TEMPLATE_PATH);
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
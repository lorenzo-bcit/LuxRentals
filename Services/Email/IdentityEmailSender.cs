using System.Reflection;
using FluentEmail.Core;
using LuxRentals.ViewModels.Email;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace LuxRentals.Services.Email;

public class IdentityEmailSender : IEmailSender
{
    private const string TEMPLATE_RESOURCE_NAME = "LuxRentals.Views.Email.IdentityEmail.cshtml";
    private static readonly string TemplateContent = LoadTemplate();

    private readonly IFluentEmailFactory _emailFactory;

    public IdentityEmailSender(IFluentEmailFactory emailFactory)
    {
        _emailFactory = emailFactory;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _emailFactory
            .Create()
            .To(email)
            .Subject(subject)
            .UsingTemplate(TemplateContent, new IdentityEmailTemplateModel
            {
                Subject = subject,
                HtmlMessage = new HtmlString(htmlMessage)
            }, true)
            .SendAsync();
    }

    private static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(TEMPLATE_RESOURCE_NAME)
            ?? throw new InvalidOperationException($"Embedded email template '{TEMPLATE_RESOURCE_NAME}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
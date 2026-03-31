using Microsoft.AspNetCore.Html;

namespace LuxRentals.ViewModels.Email;

public sealed class IdentityEmailTemplateModel
{
    public string Subject { get; init; } = string.Empty;
    public HtmlString HtmlMessage { get; init; } = HtmlString.Empty;
}
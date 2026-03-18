using Microsoft.AspNetCore.Html;

namespace LuxRentals.Services;

public sealed class IdentityEmailTemplateModel
{
    public string Subject { get; init; } = string.Empty;
    public HtmlString HtmlMessage { get; init; } = HtmlString.Empty;
}
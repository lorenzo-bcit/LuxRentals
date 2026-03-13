using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.Utils;

public static class SelectListItems
{
    public static List<SelectListItem> Build<T>(
        IEnumerable<T> items,
        Func<T, string> textSelector,
        Func<T, string> valueSelector,
        Func<T, bool> isSelected,
        string? emptyText = null,
        bool emptySelected = false,
        string emptyValue = "")
    {
        var options = new List<SelectListItem>();

        if (emptyText is not null)
            options.Add(new SelectListItem(emptyText, emptyValue, emptySelected));

        options.AddRange(items.Select(x =>
            new SelectListItem(textSelector(x), valueSelector(x), isSelected(x))));

        return options;
    }
}

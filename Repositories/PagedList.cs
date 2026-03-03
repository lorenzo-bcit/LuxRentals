using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories;

/// <summary>
/// Generic pagination helper.
/// Assumes filtering and ordering are applied before calling CreateAsync.
/// </summary>
public class PagedList<T>
{
    private PagedList(List<T> items, int page, int pageSize, int totalCount, int totalPages)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }

    public List<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();

        // Hard safety clamp (defensive)
        pageSize = Math.Clamp(pageSize, 1, 25);

        // Compute total pages (min 1) to avoid zero-page and invalid clamp ranges.
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

        // Clamp page to [1, totalPages] to guard against invalid or tampered query values.
        page = Math.Clamp(page, 1, totalPages);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, page, pageSize, totalCount, totalPages);
    }
}
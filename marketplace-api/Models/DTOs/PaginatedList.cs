namespace MarketplaceApi.Models.DTOs;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public static PaginatedList<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        var size = pageSize <= 0 ? 12 : pageSize;
        var totalPages = size == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)size);
        return new PaginatedList<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Page = page,
            PageSize = size
        };
    }
}

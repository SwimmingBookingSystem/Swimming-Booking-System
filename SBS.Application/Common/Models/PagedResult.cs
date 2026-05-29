namespace SBS.Application.Common.Models;

/// <summary>Generic paginated result wrapper used by all list queries.</summary>
public class PagedResult<T>
{
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}

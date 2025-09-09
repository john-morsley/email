namespace Morsley.UK.Email.Common.Models;

public class PaginatedResponse<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Number of items returned in current page
    /// </summary>
    public int Count => Items.Count();
}

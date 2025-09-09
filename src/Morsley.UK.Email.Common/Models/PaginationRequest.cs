using System.ComponentModel.DataAnnotations;

namespace Morsley.UK.Email.Common.Models;

public class PaginationRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Calculate the number of items to skip
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
}

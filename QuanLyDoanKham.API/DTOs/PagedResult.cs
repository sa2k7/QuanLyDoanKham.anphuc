using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    /// <summary>
    /// Generic paginated result wrapper used by collection endpoints.
    /// Returns an empty Items list (never null) when no data exists.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}

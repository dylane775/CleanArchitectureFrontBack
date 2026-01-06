using System.Collections.Generic;

namespace Identity.Application.Common.Models
{
    /// <summary>
    /// Represents a paginated list of items
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    public class PaginatedItems<T>
    {
        /// <summary>
        /// Current page index (1-based)
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Items in the current page
        /// </summary>
        public IEnumerable<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Creates a new paginated items instance
        /// </summary>
        public PaginatedItems()
        {
        }

        /// <summary>
        /// Creates a new paginated items instance with specified values
        /// </summary>
        public PaginatedItems(int pageIndex, int pageSize, long count, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }
    }
}

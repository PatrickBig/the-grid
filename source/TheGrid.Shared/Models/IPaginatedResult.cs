using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGrid.Shared.Models
{
    /// <summary>
    /// Data structure representing a paginated list of items.
    /// </summary>
    /// <typeparam name="TItem">Type of item contained in the result set.</typeparam>
    public interface IPaginatedResult<TItem>
    {
        /// <summary>
        /// Total number of items in the results.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// One page of items.
        /// </summary>
        public IEnumerable<TItem> Items { get; set; }
    }
}

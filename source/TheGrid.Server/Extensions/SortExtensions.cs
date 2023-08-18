using System.Text;
using TheGrid.Shared.Models;

namespace TheGrid.Server.Extensions
{
    public static class PaginatedResultSortDefinitionExtensions
    {
        //public static string GetSortStatement(this PaginatedResultSortDefinition sort)
        public static string GetSortStatement(this IEnumerable<Sort> sort)
        {
            var sortSegments = sort.Select(s => s.GetSortSegment());
            return string.Join(", ", sortSegments);
        }

        private static string GetSortSegment(this Sort sort)
        {
            var suffix = string.Empty;

            if (sort.Direction == SortDirection.Ascending)
            {
                suffix = " ASC";
            }
            else if (sort.Direction == SortDirection.Descending)
            {
                suffix = " DESC";
            }

            return sort.Field + suffix;
        }
    }
}

// <copyright file="JobQueues.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using System.Reflection;

namespace TheGrid.Models
{
    /// <summary>
    /// Names of all the job queues used by TheGrid.
    /// </summary>
    public static class JobQueues
    {
        /// <summary>
        /// Default job queue processes all things.
        /// </summary>
        public const string Default = "default";

        /// <summary>
        /// Processes query result refresh jobs.
        /// </summary>
        public const string QueryRefresh = "query-refresh";

        /// <summary>
        /// Validates that the selected queue options are valid.
        /// </summary>
        /// <param name="queues">Queues to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid queue names are specified.</exception>
        public static void ValidateQueues(string[] queues)
        {
            var validQueues = typeof(JobQueues).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(q => (string?)q.GetRawConstantValue());

            var invalidQueues = queues.Where(q => validQueues.All(x => x != q));

            if (invalidQueues.Any())
            {
                var invalidQueueNames = string.Join(", ", invalidQueues);
                throw new ArgumentOutOfRangeException(nameof(queues), $"Invalid queue(s) specified \"{invalidQueueNames}\". The only valid queue names are \"{string.Join(", ", validQueues)}\"");
            }
        }
    }
}

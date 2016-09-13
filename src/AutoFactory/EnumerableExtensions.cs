using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoFactory
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}

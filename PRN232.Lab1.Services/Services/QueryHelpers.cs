using System.Linq.Expressions;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services
{
    internal static class QueryHelpers
    {
        public static string[] SplitCsv(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static bool HasExpand(this QueryOptions options, string expand)
        {
            return SplitCsv(options.Expand)
                .Any(x => x.Equals(expand, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<(string Field, bool Descending)> GetSorts(this QueryOptions options)
        {
            return SplitCsv(options.Sort)
                .Select(sort => sort.StartsWith("-", StringComparison.Ordinal)
                    ? (sort[1..], true)
                    : (sort, false));
        }

        public static void Normalize(this QueryOptions options)
        {
            options.Page = options.Page < 1 ? 1 : options.Page;
            options.PageSize = options.PageSize < 1 ? 10 : Math.Min(options.PageSize, 100);
        }

        public static IQueryable<T> ApplySort<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool descending,
            ref bool ordered)
        {
            if (!ordered)
            {
                ordered = true;
                return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
            }

            var orderedQuery = (IOrderedQueryable<T>)query;
            return descending ? orderedQuery.ThenByDescending(keySelector) : orderedQuery.ThenBy(keySelector);
        }

        public static async Task<PagedResult<TModel>> ToPagedResultAsync<TEntity, TModel>(
            IQueryable<TEntity> query,
            QueryOptions options,
            Func<TEntity, TModel> map)
        {
            options.Normalize();

            var totalItems = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
            var entities = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                query.Skip((options.Page - 1) * options.PageSize).Take(options.PageSize));

            return new PagedResult<TModel>
            {
                Items = entities.Select(map).ToList(),
                TotalItems = totalItems,
                Page = options.Page,
                PageSize = options.PageSize
            };
        }
    }
}

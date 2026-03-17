using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;


namespace Common
{
    public static class IQueryableExtensions
    {
        private static readonly Dictionary<string, MethodInfo> MethodCache = new();

        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string? sort,
            HashSet<string>? allowedFields = null)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var sortField in sort.Split(','))
            {
                var trimmed = sortField.Trim();
                var ascending = !trimmed.StartsWith("-");
                var propertyName = ascending ? trimmed : trimmed.Substring(1);

                // ✅ Whitelist check
                if (allowedFields != null && !allowedFields.Contains(propertyName))
                    continue;

                Expression property;
                try
                {
                    property = propertyName
                        .Split('.')
                        .Aggregate<string, Expression>(parameter, Expression.PropertyOrField);
                }
                catch
                {
                    continue;
                }

                var lambda = Expression.Lambda(property, parameter);

                string methodName = orderedQuery == null
                    ? (ascending ? "OrderBy" : "OrderByDescending")
                    : (ascending ? "ThenBy" : "ThenByDescending");

                var cacheKey = $"{methodName}_{typeof(T).Name}_{property.Type.Name}";

                if (!MethodCache.TryGetValue(cacheKey, out var method))
                {
                    method = typeof(Queryable)
                        .GetMethods()
                        .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                        .MakeGenericMethod(typeof(T), property.Type);

                    MethodCache[cacheKey] = method;
                }

                orderedQuery = (IOrderedQueryable<T>)method.Invoke(
                    null,
                    new object[] { orderedQuery ?? query, lambda })!;
            }

            return orderedQuery ?? query;
        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalItems, page, pageSize);
        }
    }
}

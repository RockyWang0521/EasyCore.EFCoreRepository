using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.QueryableExtensions
{
    /// <summary>
    /// Extension methods for <see cref="IQueryable{T}"/> to add conditional filtering.
    /// </summary>
    public static class WhereExtensions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
            {
                return source.Where(predicate);
            }
            return source;
        }
    }
}

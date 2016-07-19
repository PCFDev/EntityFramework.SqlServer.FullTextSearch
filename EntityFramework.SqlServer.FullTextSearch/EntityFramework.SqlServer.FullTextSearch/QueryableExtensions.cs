using System;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.SqlServer.FullTextSearch
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Perform a CONTAINS full-text search.
        /// </summary>
        /// <typeparam name="TSource">The source query return type.</typeparam>
        /// <param name="source">The IQueryable of the database linq query.</param>
        /// <param name="selector">The object property to search, or the object itself for a wildcard search.</param>
        /// <param name="predicate">The search predicate.</param>
        /// <returns>An IQueryable for linq chaining.</returns>
        /// <exception cref="System.ArgumentNullException">If there is no search predicate.</exception>
        /// <exception cref="System.ArgumentException">If the search predicate is empty.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a pattern for LINQ methods.")]
        public static IQueryable<TSource> ContainsSearch<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, object>> selector, string predicate) where TSource : class
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (String.IsNullOrEmpty(predicate))
                throw new ArgumentException(Resource.EmptySearch, "predicate");

            return FreeTextSearchImp(source, selector, FullTextTags.Contains(predicate));
        }

        /// <summary>
        /// Perform a FREETEXT full-text search.
        /// </summary>
        /// <typeparam name="TSource">The source query return type.</typeparam>
        /// <param name="source">The IQueryable of the database linq query.</param>
        /// <param name="selector">The object property to search, or the object itself for a wildcard search.</param>
        /// <param name="predicate">The search predicate.</param>
        /// <returns>An IQueryable for linq chaining.</returns>
        /// <exception cref="System.ArgumentNullException">If there is no search predicate.</exception>
        /// <exception cref="System.ArgumentException">If the search predicate is empty.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a pattern for LINQ methods.")]
        public static IQueryable<TSource> FreeTextSearch<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, object>> selector, string predicate) where TSource : class
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (String.IsNullOrEmpty(predicate))
                throw new ArgumentException(Resource.EmptySearch, "predicate");

            return FreeTextSearchImp(source, selector, FullTextTags.FreeText(predicate));
        }

        //private static bool IsMultiColumn<TSource>(Expression<Func<TSource, object>> selector)
        //{
        //    if (selector is LambdaExpression == false)
        //        return false;
        //    if (selector.Body is NewExpression == false)
        //        return false;
        //    if ((selector as LambdaExpression).Parameters.Count != 1)
        //        return false;
        //    if ((selector.Body as NewExpression).Arguments.Count <= 1)
        //        return false;
        //    return true;
        //}

        private static IQueryable<TSource> FreeTextSearchImp<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, object>> selector, string predicate)
        {
            //TODO: Handle multi-column selectors, e.g. _ => new { _.Column1, _.Column2 }
            //TODO: Handle issues with querying values from linked tables.
            //var multiple = expression.Body is NewExpression ? (expression.Body as NewExpression).Arguments.Count > 1 : false;

            //If they want to use a wildcard they can either use a literal (_ => "*") or the object as the selector (_ => _).
            if (typeof(TSource) == selector.Body.Type)
                selector = (_) => "*";

            //else if (IsMultiColumn(selector))
            //{
            //    var lambda = selector as LambdaExpression;
            //    var newExp = selector.Body as NewExpression;

            //    var members = newExp.Arguments.Cast<MemberExpression>();
            //    if (members.Any(_ => _.Expression != lambda.Parameters[0]))
            //        throw new Exception();

            //    var _methodCallExpression = source.Expression;
            //    var _searchTermExpression = Expression.Property(Expression.Constant(new { Value = predicate }), "Value");
            //    foreach (var member in members)
            //    {
            //        var _checkContainsExpression = Expression.Call(member, typeof(string).GetMethod("Contains"), _searchTermExpression);
            //        _methodCallExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, _methodCallExpression, Expression.Lambda<Func<TEntity, bool>>(_checkContainsExpression, expression.Parameters));
            //    }
            //    return source.Provider.CreateQuery<TSource>(_methodCallExpression);
            //}

            var searchProperty = Expression.Property(Expression.Constant(new { Value = predicate }), "Value");
            var searchContains = Expression.Call(selector.Body, typeof(string).GetMethod("Contains"), searchProperty);
            var searchExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression, Expression.Lambda<Func<TSource, bool>>(searchContains, selector.Parameters));
            return source.Provider.CreateQuery<TSource>(searchExpression);
        }
    }
}
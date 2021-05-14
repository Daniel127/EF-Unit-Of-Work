using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Collections
{
    /// <summary>
    /// Extension methods for create paged collection.
    /// </summary>
    public static class Extensions
    {

        #region Paged Array
        /// <summary>
        /// Converts the specified source to <see cref="IPagedCollection{T}"/> by the specified <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An instance of the inherited from <see cref="IPagedCollection{T}"/> interface.</returns>
        /// <exception cref="ArgumentException">The parameters are incorrect.</exception>
        /// <exception cref="PageNotFoundException">The page desired not found.</exception>
        public static IPagedCollection<T> ToPagedArray<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            (int count, int totalPages, IQueryable<T> pagedItems) = GetPage(source, pageSize, pageNumber);
            var pageItems = pagedItems.ToArray();
            return new PagedArray<T>(pageNumber, pageSize, count, totalPages, pageItems);
        }

        /// <inheritdoc cref="ToPagedList{T}"/>
        public static Task<IPagedCollection<T>> ToPagedArrayAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            return source.ToPagedArrayInternalAsync(pageSize, pageNumber);
        }

        private static async Task<IPagedCollection<T>> ToPagedArrayInternalAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            (int count, int totalPages, IQueryable<T> pagedItems) = await GetPageAsync(source, pageSize, pageNumber);
            var pageItems = await pagedItems.ToArrayAsync();
            return new PagedArray<T>(pageNumber, pageSize, count, totalPages, pageItems);
        }
        #endregion

        #region Paged List
        /// <summary>
        /// Converts the specified source to <see cref="IPagedCollection{T}"/> by the specified <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An instance of the inherited from <see cref="IPagedCollection{T}"/> interface.</returns>
        /// <exception cref="ArgumentException">The parameters are incorrect.</exception>
        /// <exception cref="PageNotFoundException">The page desired not found.</exception>
        public static IPagedCollection<T> ToPagedList<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            (int count, int totalPages, IQueryable<T> pagedItems) = GetPage(source, pageSize, pageNumber);
            var pageItems = pagedItems.ToList();
            return new PagedList<T>(pageNumber, pageSize, count, totalPages, pageItems);
        }

        /// <inheritdoc cref="ToPagedList{T}"/>
        public static Task<IPagedCollection<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            return source.ToPagedListInternalAsync(pageSize, pageNumber);
        }

        private static async Task<IPagedCollection<T>> ToPagedListInternalAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber = 0)
        {
            (int count, int totalPages, IQueryable<T> pagedItems) = await GetPageAsync(source, pageSize, pageNumber);
            var pageItems = await pagedItems.ToListAsync();
            return new PagedList<T>(pageNumber, pageSize, count, totalPages, pageItems);
        }
        #endregion

        #region Paged Dictionary
        /// <summary>
        /// Converts the specified source to <see cref="IPagedCollection{KeyValuePair}"/> by the specified <paramref name="pageNumber"/> and <paramref name="pageSize"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the item.</typeparam>
        /// <param name="source">The source to paging.</param>
        /// <param name="pageNumber">The number of the page.</param>
        /// <param name="keySelector">The key selector</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <returns>An instance of the inherited from <see cref="IPagedCollection{KeyValuePair}"/> interface.</returns>
        /// <exception cref="ArgumentException">The parameters are incorrect.</exception>
        /// <exception cref="PageNotFoundException">The page desired not found.</exception>
        public static IPagedCollection<KeyValuePair<TKey, TValue>> ToPagedDictionary<TKey, TValue>(this IQueryable<TValue> source, Func<TValue, TKey> keySelector, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            (int count, int totalPages, IQueryable<TValue> pagedItems) = GetPage(source, pageSize, pageNumber);
            var pageItems = pagedItems.ToDictionary(keySelector);
            return new PagedDictionary<TKey, TValue>(pageNumber, pageSize, count, totalPages, pageItems);
        }

        /// <inheritdoc cref="ToPagedDictionary{TKey,TValue}"/>
        public static Task<IPagedCollection<KeyValuePair<TKey, TValue>>> ToPagedDictionaryAsync<TKey, TValue>(this IQueryable<TValue> source, Func<TValue, TKey> keySelector, int pageSize, int pageNumber = 0)
        {
            ValidateParameters(pageNumber, pageSize);
            return source.ToPagedDictionaryInternalAsync(keySelector, pageSize, pageNumber);
        }

        private static async Task<IPagedCollection<KeyValuePair<TKey, TValue>>> ToPagedDictionaryInternalAsync<TKey, TValue>(this IQueryable<TValue> source, Func<TValue, TKey> keySelector, int pageSize, int pageNumber = 0)
        {
            (int count, int totalPages, IQueryable<TValue> pagedItems) = await GetPageAsync(source, pageSize, pageNumber);
            var pageItems = await pagedItems.ToDictionaryAsync(keySelector);
            return new PagedDictionary<TKey, TValue>(pageNumber, pageSize, count, totalPages, pageItems);
        }
        #endregion

        #region Collection

        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source as IReadOnlyCollection<T> ?? new ReadOnlyCollectionWrapper<T>(source);
        }

        private sealed class ReadOnlyCollectionWrapper<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> _source;
            public ReadOnlyCollectionWrapper(ICollection<T> source) => _source = source;
            public int Count => _source.Count;
            public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion

        #region Common
        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void ValidateParameters(int pageNumber, int pageSize)
        {
            if (pageNumber < 0)
            {
                throw new ArgumentException("The page number must be greater than or equal to zero");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("The page size must be greater than zero");
            }
        }

        private static (int count, int totalPages, IQueryable<T> pagedItems) GetPage<T>(IQueryable<T> source, int pageSize, int pageNumber)
        {
            int count = source.Count();
            int totalPages = (int)Math.Ceiling((double)count / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (pageNumber >= totalPages) { throw new PageNotFoundException(pageNumber, totalPages); }
            var pagedItems = source
                .Skip(pageNumber * pageSize)
                .Take(pageSize);
            return (count, totalPages, pagedItems);
        }

        private static async Task<(int count, int totalPages, IQueryable<T> pagedItems)> GetPageAsync<T>(IQueryable<T> source, int pageSize, int pageNumber)
        {
            int count = await source.CountAsync();
            int totalPages = (int)Math.Ceiling((double)count / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (pageNumber >= totalPages) { throw new PageNotFoundException(pageNumber, totalPages); }
            var pagedItems = source
                .Skip(pageNumber * pageSize)
                .Take(pageSize);
            return (count, totalPages, pagedItems);
        }
        #endregion
    }
}

using System.Collections.Generic;

namespace QD.EntityFrameworkCore.UnitOfWork.Collections
{
    /// <summary>
    /// Paged list of <see cref="T"/>
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public class PagedList<T> : PagedCollection<IReadOnlyList<T>, T>
    {
        /// <inheritdoc />
        public PagedList(int pageNumber, int pageSize, int totalCount, int totalPages, IReadOnlyList<T> items)
            : base(pageNumber, pageSize, totalCount, totalPages, items)
        {
        }

        /// <summary>
        /// List indexer.
        /// </summary>
        /// <param name="index">Item index</param>
        public T this[int index] => ((IReadOnlyList<T>)Items)[index];
    }
}

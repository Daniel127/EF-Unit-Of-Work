namespace QD.EntityFrameworkCore.UnitOfWork.Collections
{
    /// <summary>
    /// Paged array of <see cref="T"/>
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public class PagedArray<T> : PagedCollection<T[], T>
    {
        /// <inheritdoc />
        public PagedArray(int pageNumber, int pageSize, int totalCount, int totalPages, T[] items) : base(pageNumber, pageSize, totalCount, totalPages, items)
        {
        }

        /// <summary>
        /// Array indexer.
        /// </summary>
        /// <param name="index">Item index</param>
        public T this[int index] => ((T[])Items)[index];
    }
}

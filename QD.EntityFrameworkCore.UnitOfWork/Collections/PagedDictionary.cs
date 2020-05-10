using System.Collections.Generic;

namespace QD.EntityFrameworkCore.UnitOfWork.Collections
{
	/// <summary>
	/// Paged dictionary of <see cref="KeyValuePair{TKey,TValue}"/>
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The item type.</typeparam>
	public class PagedDictionary<TKey, TValue> : PagedCollection<IReadOnlyDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
	{
		/// <inheritdoc />
		public PagedDictionary(int pageNumber, int pageSize, int totalCount, int totalPages, IReadOnlyDictionary<TKey, TValue> items)
			: base(pageNumber, pageSize, totalCount, totalPages, items)
		{
		}

		/// <summary>
		/// Dictionary indexer.
		/// </summary>
		/// <param name="key">Item key.</param>
		public TValue this[TKey key] => ((IReadOnlyDictionary<TKey, TValue>)Items)[key];
	}
}

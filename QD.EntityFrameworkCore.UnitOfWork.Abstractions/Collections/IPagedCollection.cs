using System.Collections.Generic;
using System.Linq;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections
{
	/// <summary>
	/// Paged collection of <see cref="T"/>.
	/// <remarks>This collection is read only.</remarks>
	/// </summary>
	/// <typeparam name="T">The item type.</typeparam>
	public interface IPagedCollection<out T>
	{
		/// <summary>
		/// Gets the number of the current page.
		/// <remarks>Start at zero.</remarks>
		/// </summary>
		int PageNumber { get; }
		/// <summary>
		/// Gets the page size.
		/// </summary>
		int PageSize { get; }

		/// <summary>
		/// Gets the total amount of items in the full collection
		/// </summary>
		int TotalCount { get; }
		/// <summary>
		/// Gets the total pages.
		/// </summary>
		int TotalPages { get; }

		/// <summary>
		/// Gets the has previous page.
		/// </summary>
		bool HasPreviousPage { get; }

		/// <summary>
		/// Gets the has next page.
		/// </summary>
		bool HasNextPage { get; }

		/// <summary>
		/// Gets the current page items.
		/// </summary>
		IReadOnlyCollection<T> Items { get; }

		/// <summary>
		/// Default indexer.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index] => Items.ElementAt(index);
	}
}

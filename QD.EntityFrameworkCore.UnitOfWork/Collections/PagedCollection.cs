using QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections;
using System.Collections.Generic;

namespace QD.EntityFrameworkCore.UnitOfWork.Collections
{
	/// <inheritdoc cref="IPagedCollection{T}" />
	public class PagedCollection<TCollection, T> : IPagedCollection<T> where TCollection : IReadOnlyCollection<T>
	{
		/// <inheritdoc />
		public int PageNumber { get; }

		/// <inheritdoc />
		public int PageSize { get; }

		/// <inheritdoc />
		public int TotalCount { get; }

		/// <inheritdoc />
		public int TotalPages { get; }

		/// <inheritdoc />
		public bool HasPreviousPage => PageNumber > 0;
		
		/// <inheritdoc />
		public bool HasNextPage => PageNumber < TotalPages - 1;

		/// <inheritdoc />
		public IReadOnlyCollection<T> Items { get; }

		/// <summary>
		/// Construct a paged collection
		/// </summary>
		/// <param name="pageNumber">The current page number.</param>
		/// <param name="pageSize">The page size.</param>
		/// <param name="totalCount">Total amount of items in the full collection.</param>
		/// <param name="totalPages">Total pages of the collection.</param>
		/// <param name="items">The items of the current page.</param>
		public PagedCollection(int pageNumber, int pageSize, int totalCount, int totalPages, TCollection items)
		{
			PageNumber = pageNumber;
			PageSize = pageSize;
			TotalCount = totalCount;
			TotalPages = totalPages;
			Items = items;
		}
	}
}

using System;
using System.Runtime.Serialization;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections
{
	/// <summary>
	/// This occur when the page number indicated is out of range from total pages of the collection
	/// </summary>
	[Serializable]
	public sealed class PageNotFoundException : Exception
	{
		/// <summary>
		/// Page Number desired.
		/// </summary>
		public int PageNumber { get; }
		/// <summary>
		/// Total pages found.
		/// </summary>
		public int TotalPages { get; }

		/// <summary>	
		/// 
		/// </summary>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="totalPages">The total pages</param>
		public PageNotFoundException(int pageNumber, int totalPages) : base($"Not found the page {pageNumber}, the range of pages is 0 to {totalPages - 1}.")
		{
			PageNumber = pageNumber;
			TotalPages = totalPages;
		}

		private PageNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			PageNumber = info.GetInt32(nameof(PageNumber));
			TotalPages = info.GetInt32(nameof(TotalPages));
		}

		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(PageNumber), PageNumber);
			info.AddValue(nameof(TotalPages), TotalPages);
		}
	}
}

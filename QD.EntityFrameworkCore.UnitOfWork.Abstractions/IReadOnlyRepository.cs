using QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions
{
	/// <summary>
	/// Read only repository of <see cref="TEntity"/>.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IReadOnlyRepository<TEntity> where TEntity : class
	{
		#region Read
		/// <summary>
		/// Gets all entities.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <returns>The <see cref="IQueryable"/>.</returns>
		/// <exception cref="InvalidOperationException">If <see cref="TEntity"/> DbSet does not exist in DbContext</exception>
		IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
		TEntity? GetFirstOrDefault(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate.
		/// </summary>
		/// <param name="selector">The selector for projection.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
		TResult? GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? orderBy = null, bool disableTracking = true) where TResult : class;

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>Ex: This method defaults to a read-only, no-tracking query. </remarks>
		Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate.
		/// </summary>
		/// <param name="selector">The selector for projection.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>Ex: This method defaults to a read-only, no-tracking query.</remarks>
		Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TResult>, IOrderedQueryable<TResult>>? orderBy = null, bool disableTracking = true) where TResult : class;

		/// <summary>
		/// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
		/// </summary>
		/// <param name="sql">The raw SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
		IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters);

		/// <summary>
		/// Uses interpolated SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
		/// </summary>
		/// <param name="sql">The interpolated SQL.</param>
		/// <returns></returns>
		IQueryable<TEntity> FromSqlInterpolated(FormattableString sql);

		/// <summary>
		/// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
		/// </summary>
		/// <param name="keyValues">The values of the primary key for the entity to be found.</param>
		/// <returns>The found entity or null.</returns>
		TEntity Find(params object[] keyValues);

		/// <summary>
		/// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
		/// </summary>
		/// <param name="keyValues">The values of the primary key for the entity to be found.</param>
		/// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
		ValueTask<TEntity> FindAsync(params object[] keyValues);

		/// <summary>
		/// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
		/// </summary>
		/// <param name="keyValues">The values of the primary key for the entity to be found.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
		ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);
		#endregion

		#region Other
		/// <summary>
		/// Gets the count based on a predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		int Count(Expression<Func<TEntity, bool>>? predicate = null);

		/// <summary>
		/// Gets the count based on a predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns></returns>
		Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Checked exists record by predicate
		/// </summary>
		/// <param name="predicate"></param>
		bool Any(Expression<Func<TEntity, bool>>? predicate = null);
		#endregion

		#region Paged collection

		/// <summary>
		/// Get paged array of the entities.
		/// </summary>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		IPagedCollection<TEntity> GetPagedArray(int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		/// <summary>
		/// Get paged array of the entities.
		/// </summary>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		Task<IPagedCollection<TEntity>> GetPagedArrayAsync(int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		/// <summary>
		/// Get paged list of the entities.
		/// </summary>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		IPagedCollection<TEntity> GetPagedList(int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		/// <summary>
		/// Get paged list of the entities.
		/// </summary>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		Task<IPagedCollection<TEntity>> GetPagedListAsync(int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		/// <summary>
		/// Get paged dictionary of the entities.
		/// </summary>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		IPagedCollection<KeyValuePair<TKey, TEntity>> GetPagedDictionary<TKey>(Func<TEntity, TKey> keySelector, int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		/// <summary>
		/// Get paged dictionary of the entities.
		/// </summary>
		/// <param name="keySelector">The key selector.</param>
		/// <param name="pageSize">The page size.</param>
		/// <param name="pageNumber">The page number.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <returns>The desired page.</returns>
		/// <exception cref="ArgumentException">The parameters are incorrect.</exception>
		/// <exception cref="PageNotFoundException">The page desired not found.</exception>
		Task<IPagedCollection<KeyValuePair<TKey, TEntity>>> GetPagedDictionaryAsync<TKey>(Func<TEntity, TKey> keySelector, int pageSize, int pageNumber = 0, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

		#endregion
	}
}

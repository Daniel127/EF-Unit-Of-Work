using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions
{
	/// <summary>
	/// Defines the interfaces for generic repository.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IRepository<TEntity> where TEntity : class
	{
		#region Create
		/// <summary>
		/// Inserts a new entity synchronously.
		/// </summary>
		/// <param name="entity">The entity to insert.</param>
		void Insert([NotNull] TEntity entity);

		/// <summary>
		/// Inserts a range of entities synchronously.
		/// </summary>
		/// <param name="entities">The entities to insert.</param>
		void Insert([NotNull] IEnumerable<TEntity> entities);

		/// <summary>
		/// Inserts a new entity asynchronously.
		/// </summary>
		/// <param name="entity">The entity to insert.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A <see cref="Task"/> that represents the asynchronous insert operation.</returns>
		ValueTask<TEntity> InsertAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Inserts a range of entities asynchronously.
		/// </summary>
		/// <param name="entities">The entities to insert.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A <see cref="Task"/> that represents the asynchronous insert operation.</returns>
		Task InsertAsync([NotNull] IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
		#endregion

		#region Read
		/// <summary>
		/// Gets all entities.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <returns>The <see cref="IQueryable"/>.</returns>
		/// <exception cref="InvalidOperationException">If <see cref="TEntity"/> DbSet does not exist in DbContext</exception>
		IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate. This method defaults to a read-only, no-tracking query.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
		TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate. This method defaults to a read-only, no-tracking query.
		/// </summary>
		/// <param name="selector">The selector for projection.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
		TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TResult>>>? orderBy = null, bool disableTracking = true) where TResult : class;

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate. This method defaults to a read-only, no-tracking query.
		/// </summary>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>Ex: This method defaults to a read-only, no-tracking query. </remarks>
		Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true);

		/// <summary>
		/// Gets the first or default entity based on a predicate, order by delegate and include delegate. This method defaults to a read-only, no-tracking query.
		/// </summary>
		/// <param name="selector">The selector for projection.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="orderBy">A function to order elements.</param>
		/// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
		/// <remarks>Ex: This method defaults to a read-only, no-tracking query.</remarks>
		Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true) where TResult : class;

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
		TEntity Find([MaybeNull] params object[] keyValues);

		/// <summary>
		/// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
		/// </summary>
		/// <param name="keyValues">The values of the primary key for the entity to be found.</param>
		/// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
		ValueTask<TEntity> FindAsync([MaybeNull] params object[] keyValues);

		/// <summary>
		/// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
		/// </summary>
		/// <param name="keyValues">The values of the primary key for the entity to be found.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
		ValueTask<TEntity> FindAsync([MaybeNull] object[] keyValues, CancellationToken cancellationToken);
		#endregion

		#region Update
		/// <summary>
		/// Updates the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		void Update([NotNull] TEntity entity);

		/// <summary>
		/// Updates the specified entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		void Update([NotNull] params TEntity[] entities);

		/// <summary>
		/// Updates the specified entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		void Update([NotNull] IEnumerable<TEntity> entities);
		#endregion

		#region Delete
		/// <summary>
		/// Deletes the entity by the specified primary key.
		/// </summary>
		/// <param name="keyValues">The primary key value.</param>
		void Delete([MaybeNull] params object[] keyValues);

		/// <summary>
		/// Deletes the specified entity.
		/// </summary>
		/// <param name="entity">The entity to delete.</param>
		void Delete([NotNull] TEntity entity);

		/// <summary>
		/// Deletes the specified entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		void Delete([NotNull] IEnumerable<TEntity> entities);
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
	}
}

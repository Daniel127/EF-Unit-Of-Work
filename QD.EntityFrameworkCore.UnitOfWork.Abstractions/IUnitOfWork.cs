using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions
{
	/// <summary>
	/// Defines the interface for unit of work.
	/// </summary>
	public interface IUnitOfWork : IRepositoryFactory, IDisposable
	{
		/// <summary>
		/// Saves all changes made in this context to the database.
		/// </summary>
		/// <returns>The number of state entries written to the database.</returns>
		int SaveChanges();

		/// <summary>
		/// Asynchronously saves all changes made in this unit of work to the database.
		/// </summary>
		/// <returns>A <see cref="Task"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
		Task<int> SaveChangesAsync();

		/// <summary>
		/// Executes the specified interpolated SQL.
		/// </summary>
		/// <param name="sql">The interpolated SQL.</param>
		/// <returns>The number of state entities written to database.</returns>
		public int ExecuteSqlInterpolated(FormattableString sql);

		/// <summary>
		/// Executes the specified interpolated SQL.
		/// </summary>
		/// <param name="sql">The interpolated SQL.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The number of state entities written to database.</returns>
		public Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default);

		/// <summary>
		/// Executes the specified raw SQL command.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns>The number of state entities written to database.</returns>
		public int ExecuteSqlRaw(string sql, IEnumerable<object>? parameters = null);

		/// <summary>
		/// Executes the specified raw SQL command.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The number of state entities written to database.</returns>
		public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Uses interpolated SQL query to fetch the specified <typeparamref name="TEntity"/> data.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="sql">The interpolated SQL.</param>
		/// <returns>An <see cref="IQueryable"/> that contains elements that satisfy the condition specified by raw SQL.</returns>
		public IQueryable<TEntity> FromSqlInterpolated<TEntity>(FormattableString sql) where TEntity : class;

		/// <summary>
		/// Uses raw SQL query to fetch the specified <typeparamref name="TEntity"/> data.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="sql">The raw SQL.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns>An <see cref="IQueryable{T}"/> that contains elements that satisfy the condition specified by raw SQL.</returns>
		public IQueryable<TEntity> FromSqlRaw<TEntity>(string sql, params object[] parameters) where TEntity : class;
	}
}

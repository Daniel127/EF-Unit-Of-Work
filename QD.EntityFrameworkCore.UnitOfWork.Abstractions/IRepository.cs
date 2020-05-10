using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions
{
	/// <summary>
	/// Repository of <see cref="TEntity"/>.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
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
	}

}

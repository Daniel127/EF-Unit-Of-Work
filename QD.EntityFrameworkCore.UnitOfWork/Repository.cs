using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork
{
	/// <inheritdoc cref="IRepository{TEntity}" />
	public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity> where TEntity : class
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
		/// </summary>
		/// <param name="dbContext">The database context.</param>
		public Repository([NotNull] DbContext dbContext) : base(dbContext)
		{
		}

		#region Create
		/// <inheritdoc />
		public void Insert(TEntity entity)
		{
			DbSet.Add(entity);
		}

		/// <inheritdoc />
		public void Insert(IEnumerable<TEntity> entities)
		{
			DbSet.AddRange(entities);
		}

		/// <inheritdoc />
		public async ValueTask<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			return (await DbSet.AddAsync(entity, cancellationToken)).Entity;
		}

		/// <inheritdoc />
		public Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			return DbSet.AddRangeAsync(entities, cancellationToken);
		}
		#endregion

		#region Update
		/// <inheritdoc />
		public void Update([NotNull] TEntity entity)
		{
			DbSet.Update(entity);
		}

		/// <inheritdoc />
		public void Update([NotNull] params TEntity[] entities)
		{
			DbSet.UpdateRange(entities);
		}

		/// <inheritdoc />
		public void Update([NotNull] IEnumerable<TEntity> entities)
		{
			DbSet.UpdateRange(entities);
		}
		#endregion

		#region Delete
		/// <inheritdoc />
		public void Delete([MaybeNull] params object[] keyValues)
		{
			TEntity entity = DbSet.Find(keyValues);
			if (entity != null)
			{
				Delete(entity);
			}
		}

		/// <inheritdoc />
		public void Delete([NotNull] TEntity entity)
		{
			DbSet.Remove(entity);
		}

		/// <inheritdoc />
		public void Delete([NotNull] IEnumerable<TEntity> entities)
		{
			DbSet.RemoveRange(entities);
		}
		#endregion

	}
}

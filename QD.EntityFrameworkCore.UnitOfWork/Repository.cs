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
        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        /// <inheritdoc />
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);
        }

        /// <inheritdoc />
        public virtual async ValueTask<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return (await DbSet.AddAsync(entity, cancellationToken)).Entity;
        }

        /// <inheritdoc />
        public virtual Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return DbSet.AddRangeAsync(entities, cancellationToken);
        }
        #endregion

        #region Update
        /// <inheritdoc />
        public virtual void Update([NotNull] TEntity entity)
        {
            DbSet.Update(entity);
        }

        /// <inheritdoc />
        public virtual void Update([NotNull] params TEntity[] entities)
        {
            DbSet.UpdateRange(entities);
        }

        /// <inheritdoc />
        public virtual void Update([NotNull] IEnumerable<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
        }
        #endregion

        #region Delete
        /// <inheritdoc />
        public virtual void Delete([MaybeNull] params object[] keyValues)
        {
            TEntity entity = DbSet.Find(keyValues);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        /// <inheritdoc />
        public virtual void Delete([NotNull] TEntity entity)
        {
            DbSet.Remove(entity);
        }

        /// <inheritdoc />
        public virtual void Delete([NotNull] IEnumerable<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
        }
        #endregion

    }
}

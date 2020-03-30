using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork
{
	/// <inheritdoc />
	public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		/// <summary>
		/// The database context.
		/// </summary>
		protected readonly DbContext DbContext;
		/// <summary>
		/// The database set of <see cref="TEntity"/>
		/// </summary>
		protected readonly DbSet<TEntity> DbSet;

		/// <summary>
		/// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
		/// </summary>
		/// <param name="dbContext">The database context.</param>
		public Repository([NotNull] DbContext dbContext)
		{
			DbContext = dbContext;
			DbSet = DbContext.Set<TEntity>();
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

		#region Read
		/// <inheritdoc />
		public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy) : query;
		}

		/// <inheritdoc />
		public TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy).FirstOrDefault() : query.FirstOrDefault();
		}

		/// <inheritdoc />
		public TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TResult>>>? orderBy = null, bool disableTracking = true) where TResult : class
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			IQueryable<TResult> queryResult = query.Select(selector);
			return orderBy != null ? queryResult.OrderBy(orderBy).FirstOrDefault() : queryResult.FirstOrDefault();
		}

		/// <inheritdoc />
		public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy).AsQueryable().FirstOrDefaultAsync() : query.FirstOrDefaultAsync();
		}

		/// <inheritdoc />
		public Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true) where TResult : class
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			IQueryable<TResult> queryResult = query.Select(selector);
			return orderBy != null ? queryResult.OrderBy(orderBy).AsQueryable().FirstOrDefaultAsync() : queryResult.FirstOrDefaultAsync();
		}

		/// <inheritdoc />
		public IQueryable<TEntity> FromSqlInterpolated(FormattableString sql)
		{
			return DbSet.FromSqlInterpolated(sql);
		}

		/// <inheritdoc />
		public IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters)
		{
			return DbSet.FromSqlRaw(sql, parameters);
		}

		/// <inheritdoc />
		public TEntity Find(params object[] keyValues)
		{
			return DbSet.Find(keyValues);
		}

		/// <inheritdoc />
		public ValueTask<TEntity> FindAsync(params object[] keyValues)
		{
			return DbSet.FindAsync(keyValues);
		}

		/// <inheritdoc />
		public ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
		{
			return DbSet.FindAsync(keyValues, cancellationToken);
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

		#region Other
		/// <inheritdoc />
		public int Count(Expression<Func<TEntity, bool>>? predicate = null)
		{
			return predicate != null ? DbSet.Count(predicate) : DbSet.Count();
		}

		/// <inheritdoc />
		public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
		{
			return predicate != null ? DbSet.CountAsync(predicate, cancellationToken) : DbSet.CountAsync(cancellationToken);
		}

		/// <inheritdoc />
		public bool Any(Expression<Func<TEntity, bool>>? predicate = null)
		{
			return predicate != null ? DbSet.Any(predicate) : DbSet.Any();
		}
		#endregion
	}
}

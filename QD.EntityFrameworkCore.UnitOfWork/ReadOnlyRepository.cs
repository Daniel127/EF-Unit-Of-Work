using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork
{
	public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
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
		/// Initializes a new instance of the <see cref="ReadOnlyRepository{TEntity}"/> class.
		/// </summary>
		/// <param name="dbContext">The database context.</param>
		public ReadOnlyRepository([NotNull] DbContext dbContext)
		{
			DbContext = dbContext;
			DbSet = DbContext.Set<TEntity>();
		}

		#region Read
		/// <inheritdoc />
		public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy) : query;
		}

		/// <inheritdoc />
		public virtual TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy).FirstOrDefault() : query.FirstOrDefault();
		}

		/// <inheritdoc />
		public virtual TResult GetFirstOrDefault<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TResult>>>? orderBy = null, bool disableTracking = true) where TResult : class
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
		public virtual Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true)
		{
			IQueryable<TEntity> query = DbSet;
			if (disableTracking)
				query = query.AsNoTracking();
			if (predicate != null)
				query = query.Where(predicate);
			return orderBy != null ? query.OrderBy(orderBy).AsQueryable().FirstOrDefaultAsync() : query.FirstOrDefaultAsync();
		}

		/// <inheritdoc />
		public virtual Task<TResult> GetFirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TResult, IOrderedQueryable<TEntity>>>? orderBy = null, bool disableTracking = true) where TResult : class
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
		public virtual IQueryable<TEntity> FromSqlInterpolated(FormattableString sql)
		{
			return DbSet.FromSqlInterpolated(sql);
		}

		/// <inheritdoc />
		public virtual IQueryable<TEntity> FromSqlRaw(string sql, params object[] parameters)
		{
			return DbSet.FromSqlRaw(sql, parameters);
		}

		/// <inheritdoc />
		public virtual TEntity Find(params object[] keyValues)
		{
			return DbSet.Find(keyValues);
		}

		/// <inheritdoc />
		public virtual ValueTask<TEntity> FindAsync(params object[] keyValues)
		{
			return DbSet.FindAsync(keyValues);
		}

		/// <inheritdoc />
		public virtual ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
		{
			return DbSet.FindAsync(keyValues, cancellationToken);
		}
		#endregion

		#region Other
		/// <inheritdoc />
		public virtual int Count(Expression<Func<TEntity, bool>>? predicate = null)
		{
			return predicate != null ? DbSet.Count(predicate) : DbSet.Count();
		}

		/// <inheritdoc />
		public virtual Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
		{
			return predicate != null ? DbSet.CountAsync(predicate, cancellationToken) : DbSet.CountAsync(cancellationToken);
		}

		/// <inheritdoc />
		public virtual bool Any(Expression<Func<TEntity, bool>>? predicate = null)
		{
			return predicate != null ? DbSet.Any(predicate) : DbSet.Any();
		}
		#endregion
	}
}

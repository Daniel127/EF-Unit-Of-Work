﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace QD.EntityFrameworkCore.UnitOfWork
{
	/// <summary>
	/// Implementation of <see cref="IUnitOfWork"/> and <see cref="IUnitOfWork{TContext}"/>
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext, IDbContext
	{
		/// <inheritdoc />
		public TContext DbContext { get; }

		private bool Disposed { get; set; }
		private object SyncRoot { get; }
		private IDictionary<Type, object> Repositories { get; }
		private ILogger<IUnitOfWork<TContext>>? Logger { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public UnitOfWork(TContext context)
		{
			DbContext = context ?? throw new ArgumentNullException(nameof(context));
			Disposed = false;
			SyncRoot = new object();
			Repositories = new Dictionary<Type, object>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class with Logger Support.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="logger">The logger.</param>
		public UnitOfWork(TContext context, ILogger<IUnitOfWork<TContext>> logger) : this(context)
		{
			Logger = logger;
		}

		/// <inheritdoc />
		public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
		{
			lock (SyncRoot)
			{
				Type entityType = typeof(TEntity);
				if (Repositories.ContainsKey(entityType)) return (IRepository<TEntity>)Repositories[entityType];

				try
				{
					IRepository<TEntity> customRepo = DbContext.GetService<IRepository<TEntity>>();
					Repositories[entityType] = customRepo;
					return customRepo;
				}
				catch
				{
					Logger?.LogDebug("Can't get Repository from service provider");
				}

				Repositories[entityType] = new Repository<TEntity>(DbContext);
				return (IRepository<TEntity>)Repositories[entityType];
			}
		}

		#region Execute SQL
		/// <inheritdoc />
		public int ExecuteSqlInterpolated(FormattableString sql)
		{
			return DbContext.Database.ExecuteSqlInterpolated(sql);
		}

		/// <inheritdoc />
		public async Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default)
		{
			return await DbContext.Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
		}

		/// <inheritdoc />
		public int ExecuteSqlRaw(string sql, IEnumerable<object>? parameters = null)
		{
			return parameters == null
				? DbContext.Database.ExecuteSqlRaw(sql)
				: DbContext.Database.ExecuteSqlRaw(sql, parameters);
		}

		/// <inheritdoc />
		public async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default)
		{
			return parameters == null
				? await DbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken)
				: await DbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
		}
		#endregion

		#region From SQL
		/// <inheritdoc />
		public IQueryable<TEntity> FromSqlInterpolated<TEntity>(FormattableString sql) where TEntity : class
		{
			IRepository<TEntity> repository = GetRepository<TEntity>();
			return repository.FromSqlInterpolated(sql);
		}

		/// <inheritdoc />
		public IQueryable<TEntity> FromSqlRaw<TEntity>(string sql, params object[] parameters) where TEntity : class
		{
			IRepository<TEntity> repository = GetRepository<TEntity>();
			return repository.FromSqlRaw(sql, parameters);
		}
		#endregion

		#region Save Changes
		/// <inheritdoc />
		public int SaveChanges()
		{
			try
			{
				Logger?.LogDebug("Saving Changes");
				int changes = DbContext.SaveChanges();
				Logger?.LogDebug("Saved Changes");
				return changes;
			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Error in {0}", nameof(SaveChanges));
				throw;
			}
		}

		/// <inheritdoc />
		public async Task<int> SaveChangesAsync()
		{
			try
			{
				Logger?.LogDebug("Saving Changes");
				int changes = await DbContext.SaveChangesAsync();
				Logger?.LogDebug("Saved Changes");
				return changes;
			}
			catch (Exception e)
			{
				Logger?.LogError(e, "Error in {0}", nameof(SaveChangesAsync));
				throw;
			}
		}

		/// <inheritdoc />
		public async Task<int> SaveChangesAsync(params IUnitOfWork[] unitOfWorks)
		{
			try
			{
				using TransactionScope transaction = new TransactionScope();
				int count = 0;
				foreach (IUnitOfWork unitOfWork in unitOfWorks)
				{
					count += await unitOfWork.SaveChangesAsync();
				}
				count += await SaveChangesAsync();

				transaction.Complete();
				return count;
			}
			catch (Exception e)
			{
				const string name = nameof(SaveChangesAsync);
				Logger?.LogError(e, "Error in {name}", name);
				throw;
			}
		}
		#endregion

		#region Dispose
		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">The disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			lock (SyncRoot)
			{
				if (!Disposed && disposing)
				{
					Repositories.Clear();
					DbContext.Dispose();
				}
				Disposed = true;
			}
		}
		#endregion
	}
}
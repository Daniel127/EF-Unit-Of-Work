using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System;
using System.Collections.Concurrent;
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
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public TContext DbContext { get; }

        private bool Disposed { get; set; }
        private object SyncRoot { get; }
        private IDictionary<Type, object> Repositories { get; }
        private IDictionary<Type, object> ReadOnlyRepositories { get; }
        private ILogger<IUnitOfWork<TContext>>? Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serviceProvider">The application service provider</param>
        public UnitOfWork(TContext context, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = context;
            Disposed = false;
            SyncRoot = new object();
            Repositories = new ConcurrentDictionary<Type, object>();
            ReadOnlyRepositories = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class with Logger Support.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serviceProvider">The application service provider</param>
        /// <param name="logger">The logger.</param>
        public UnitOfWork(TContext context, IServiceProvider serviceProvider, ILogger<IUnitOfWork<TContext>> logger) : this(context, serviceProvider)
        {
            Logger = logger;
        }

        /// <inheritdoc />
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            Type entityType = typeof(TEntity);
            if (Repositories.ContainsKey(entityType))
            {
                Logger?.LogDebug($"Get existing Repository for entity {typeof(TEntity).Name}");
                return (IRepository<TEntity>)Repositories[entityType];
            }

            try
            {
                Logger?.LogDebug($"Get Repository for entity {typeof(TEntity).Name} from services");
                IRepository<TEntity> customRepo = (IRepository<TEntity>) _serviceProvider.GetService(typeof(IRepository<TEntity>));
                Repositories[entityType] = customRepo ?? throw new Exception("Service null");
                return customRepo;
            }
            catch(Exception e)
            {
                Logger?.LogDebug("Can't get Repository from service provider: {0}", e.Message);
            }
            Logger?.LogDebug($"Creating new Repository for entity {typeof(TEntity).Name}");
            Repositories[entityType] = new Repository<TEntity>(DbContext);
            return (IRepository<TEntity>)Repositories[entityType];
        }

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>() where TEntity : class
        {
            Type entityType = typeof(TEntity);
            if (ReadOnlyRepositories.ContainsKey(entityType))
            {
                Logger?.LogDebug($"Get existing ReadOnlyRepository for entity {typeof(TEntity).Name}");
                return (IReadOnlyRepository<TEntity>)ReadOnlyRepositories[entityType];
            }

            try
            {
                Logger?.LogDebug($"Get ReadOnlyRepository for entity {typeof(TEntity).Name} from services");
                IReadOnlyRepository<TEntity> customRepo = DbContext.GetService<IReadOnlyRepository<TEntity>>();
                ReadOnlyRepositories[entityType] = customRepo;
                return customRepo;
            }
            catch
            {
                Logger?.LogDebug("Can't get ReadOnlyRepository from service provider");
            }
            Logger?.LogDebug($"Creating new ReadOnlyRepository for entity {typeof(TEntity).Name}");
            ReadOnlyRepositories[entityType] = new ReadOnlyRepository<TEntity>(DbContext);
            return (IReadOnlyRepository<TEntity>)ReadOnlyRepositories[entityType];
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
                Logger?.LogDebug($"Saving Changes in {GetType().GetFriendlyName()}");
                int changes = DbContext.SaveChanges();
                Logger?.LogDebug($"Saved Changes in {GetType().GetFriendlyName()}");
                return changes;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error in {0}", nameof(SaveChanges));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Logger?.LogDebug($"Saving Changes in {GetType().GetFriendlyName()}");
                int changes = await DbContext.SaveChangesAsync(cancellationToken);
                Logger?.LogDebug($"Saved Changes in {GetType().GetFriendlyName()}");
                return changes;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error in {0}", nameof(SaveChangesAsync));
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(IEnumerable<IUnitOfWork> unitOfWorks, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger?.LogDebug("Beginning transaction");
                using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
                int count = 0;
                foreach (IUnitOfWork unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                count += await SaveChangesAsync(cancellationToken);
                transaction.Complete();
                Logger?.LogDebug("Finalizing transaction");
                return count;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error in {0}", $"{nameof(SaveChangesAsync)}({nameof(unitOfWorks)})");
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
                    Logger?.LogTrace("Disposing {0}", GetType().GetFriendlyName());
                    Repositories.Clear();
                    ReadOnlyRepositories.Clear();
                    DbContext.Dispose();
                }
                Disposed = true;
            }
        }
        #endregion
    }
}

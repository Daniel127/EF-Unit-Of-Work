using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QD.EntityFrameworkCore.UnitOfWork.Abstractions
{
    /// <summary>
    /// Defines the interface(s) for generic unit of work.
    /// </summary>
    public interface IUnitOfWork<out TContext> : IUnitOfWork where TContext : IDbContext
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        TContext DbContext { get; }

        /// <summary>
        /// Saves all changes made in this context to the database with distributed transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <param name="unitOfWorks">An optional <see cref="IEnumerable{IUnitOfWork}"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        Task<int> SaveChangesAsync(IEnumerable<IUnitOfWork> unitOfWorks, CancellationToken cancellationToken = default);
    }
}

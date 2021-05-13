#nullable enable
using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using System;
using System.Linq.Expressions;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        /// <inheritdoc />
        public ProductRepository(DbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public override int Count(Expression<Func<Product, bool>>? predicate = null)
        {
            return 127;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts
{
    public class TestDbContext2 : DbContext, IDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        public TestDbContext2(DbContextOptions<TestDbContext2> options) : base(options)
        {
        }
    }
}

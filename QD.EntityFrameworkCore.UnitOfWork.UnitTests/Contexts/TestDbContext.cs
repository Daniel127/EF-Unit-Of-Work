using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts
{
	public class TestDbContext : DbContext
	{
		public DbSet<Product> Products { get; set; }

		public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
		{
		}
	}
}

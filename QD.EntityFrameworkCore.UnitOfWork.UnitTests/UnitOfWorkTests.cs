using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests
{
	public class UnitOfWorkTests : IDisposable
	{
		private readonly IUnitOfWork<TestDbContext> _unitOfWorkProducts;
		private readonly IUnitOfWork<TestDbContext2> _unitOfWorkUsers;

		public UnitOfWorkTests(ITestOutputHelper output)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<ILogger<IUnitOfWork<TestDbContext>>>(new XUnitLogger<IUnitOfWork<TestDbContext>>(output));
			services.AddSingleton<DbContext>(provider => provider.GetService<TestDbContext>());
			services.AddDbContext<TestDbContext>(builder =>
			{
				builder.UseInMemoryDatabase("TestUnitOfWork");
			});
			services.AddDbContext<TestDbContext2>(builder =>
			{
				builder.UseInMemoryDatabase("TestUnitOfWork2");
			});

			services.AddSingleton<IRepository<Product>, Repository<Product>>(); //Custom Repository
			services.AddSingleton<IUnitOfWork<TestDbContext>, UnitOfWork<TestDbContext>>();
			services.AddSingleton<IUnitOfWork<TestDbContext2>, UnitOfWork<TestDbContext2>>();

			ServiceProvider serviceProvider = services.BuildServiceProvider();
			_unitOfWorkProducts = serviceProvider.GetService<IUnitOfWork<TestDbContext>>();
			_unitOfWorkUsers = serviceProvider.GetService<IUnitOfWork<TestDbContext2>>();
		}

		[Fact]
		public void GetRepository()
		{
			var userRepository = _unitOfWorkUsers.GetRepository<User>();
			userRepository.Should().NotBeNull();
			userRepository.Any().Should().BeFalse();
		}

		[Fact]
		public void GetRepositoryCreated()
		{
			var userRepository = _unitOfWorkUsers.GetRepository<User>();
			userRepository.Should().NotBeNull();
			userRepository.Any().Should().BeFalse();

			var userRepository2 = _unitOfWorkUsers.GetRepository<User>();
			userRepository2.Should().NotBeNull().And.Be(userRepository);
			userRepository2.Any().Should().BeFalse();
		}

		[Fact]
		public void GetCustomRepository()
		{
			var productRepository = _unitOfWorkProducts.GetRepository<Product>();
			productRepository.Should().NotBeNull();
			productRepository.Any().Should().BeFalse();
		}

		[Fact]
		public void SaveChanges()
		{
			var productRepository = _unitOfWorkProducts.GetRepository<Product>();
			productRepository.Any().Should().BeFalse();
			productRepository.Insert(new Product { Name = "Product1", Price = 100 });
			productRepository.Any().Should().BeFalse();
			_unitOfWorkProducts.SaveChanges();
			productRepository.Any().Should().BeTrue();
		}

		[Fact]
		public async Task SaveChangesAsync()
		{
			var productRepository = _unitOfWorkProducts.GetRepository<Product>();
			productRepository.Any().Should().BeFalse();
			await productRepository.InsertAsync(new Product { Name = "Product1", Price = 100 });
			productRepository.Any().Should().BeFalse();
			await _unitOfWorkProducts.SaveChangesAsync();
			productRepository.Any().Should().BeTrue();
		}

		[Fact]
		public async Task SaveChangesAsyncMultiple()
		{
			var productRepository = _unitOfWorkProducts.GetRepository<Product>();
			var productRepository2 = _unitOfWorkProducts.GetRepository<Product>();
			productRepository.Any().Should().BeFalse();
			productRepository2.Any().Should().BeFalse();
			await productRepository.InsertAsync(new Product { Name = "Product1", Price = 100 });
			await productRepository2.InsertAsync(new Product { Name = "Product2", Price = 200 });
			productRepository.Any().Should().BeFalse();
			productRepository2.Any().Should().BeFalse();
			await _unitOfWorkProducts.SaveChangesAsync(_unitOfWorkUsers);
			productRepository.Any().Should().BeTrue();
			productRepository2.Any().Should().BeTrue();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_unitOfWorkProducts.DbContext.Products.RemoveRange(_unitOfWorkProducts.DbContext.Products);
			_unitOfWorkProducts.DbContext.SaveChanges();
			_unitOfWorkProducts.Dispose();

			_unitOfWorkUsers.DbContext.Users.RemoveRange(_unitOfWorkUsers.DbContext.Users);
			_unitOfWorkUsers.DbContext.Products.RemoveRange(_unitOfWorkUsers.DbContext.Products);
			_unitOfWorkUsers.DbContext.SaveChanges();
			_unitOfWorkUsers.Dispose();
		}
	}
}

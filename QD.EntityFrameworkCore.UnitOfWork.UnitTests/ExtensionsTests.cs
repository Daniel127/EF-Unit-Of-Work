using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Repositories;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests
{
	public class ExtensionsTests
	{
		private readonly IServiceCollection _services;

		public ExtensionsTests(ITestOutputHelper output)
		{
			_services = new ServiceCollection();
			_services.AddSingleton<ILogger<IUnitOfWork<TestDbContext>>>(new XUnitLogger<IUnitOfWork<TestDbContext>>(output));
			_services.AddSingleton<DbContext>(provider => provider.GetService<TestDbContext>());
			_services.AddDbContext<TestDbContext>(builder =>
			{
				builder.UseInMemoryDatabase("TestUnitOfWork");
			});
			_services.AddDbContext<TestDbContext2>(builder =>
			{
				builder.UseInMemoryDatabase("TestUnitOfWork");
			});
		}


		[Theory]
		[InlineData(ServiceLifetime.Singleton, true)]
		[InlineData(ServiceLifetime.Singleton, false)]
		[InlineData(ServiceLifetime.Scoped, true)]
		[InlineData(ServiceLifetime.Scoped, false)]
		[InlineData(ServiceLifetime.Transient, true)]
		[InlineData(ServiceLifetime.Transient, false)]
		public void RegisterUnitOfWork(ServiceLifetime lifeTime, bool onlyGeneric)
		{
			_services.AddUnitOfWork<TestDbContext>(lifeTime, onlyGeneric);
			ServiceProvider serviceProvider = _services.BuildServiceProvider();
			serviceProvider.Should().NotBeNull();

			Func<IUnitOfWork> getService = () => serviceProvider.GetService<IUnitOfWork>();
			Func<IUnitOfWork<TestDbContext>> getServiceGeneric = () => serviceProvider.GetService<IUnitOfWork<TestDbContext>>();

			if (onlyGeneric)
			{
				IUnitOfWork unitOfWork = getService.Should().NotThrow().Subject;
				unitOfWork.Should().BeNull();
			}
			else
			{
				IUnitOfWork unitOfWork = getService.Should().NotThrow().Subject;
				unitOfWork.Should().NotBeNull();

				IRepository<Product> repo = unitOfWork.GetRepository<Product>();
				repo.Should().NotBeNull();

				repo.Any().Should().BeFalse();
			}

			IUnitOfWork<TestDbContext> unitOfWorkTestDb = getServiceGeneric.Should().NotThrow().Subject;
			unitOfWorkTestDb.Should().NotBeNull();

			IRepository<Product> repository = unitOfWorkTestDb.GetRepository<Product>();
			repository.Should().NotBeNull();

			repository.Any().Should().BeFalse();
		}

		[Theory]
		[InlineData(ServiceLifetime.Singleton, true)]
		[InlineData(ServiceLifetime.Singleton, false)]
		[InlineData(ServiceLifetime.Scoped, true)]
		[InlineData(ServiceLifetime.Scoped, false)]
		[InlineData(ServiceLifetime.Transient, true)]
		[InlineData(ServiceLifetime.Transient, false)]
		public void RegisterMultipleUnitOfWork(ServiceLifetime lifeTime, bool onlyGeneric)
		{
			_services.AddUnitOfWork<TestDbContext>(lifeTime, onlyGeneric);
			_services.AddUnitOfWork<TestDbContext2>(lifeTime, onlyGeneric);
			ServiceProvider serviceProvider = _services.BuildServiceProvider();
			serviceProvider.Should().NotBeNull();


			IEnumerable<IUnitOfWork> servicesUnitOfWork = serviceProvider.GetServices<IUnitOfWork>();
			IEnumerable<IUnitOfWork<TestDbContext>> servicesTestDb = serviceProvider.GetServices<IUnitOfWork<TestDbContext>>();
			IEnumerable<IUnitOfWork<TestDbContext2>> servicesTestDb2 = serviceProvider.GetServices<IUnitOfWork<TestDbContext2>>();

			int count = onlyGeneric ? 0 : 2;
			servicesUnitOfWork.Should().HaveCount(count);
			servicesTestDb.Should().OnlyHaveUniqueItems();
			servicesTestDb2.Should().OnlyHaveUniqueItems();

			IUnitOfWork unitOfWork = serviceProvider.GetService<IUnitOfWork>();
			if (onlyGeneric)
			{
				unitOfWork.Should().BeNull();
			}
			else
			{
				unitOfWork.Should().NotBeNull().And.BeAssignableTo<IUnitOfWork<TestDbContext2>>();
			}
		}

		[Fact]
		public void RegisterCustomRepository()
		{
			_services.AddRepository<Product, ProductRepository>();
			ServiceProvider serviceProvider = _services.BuildServiceProvider();
			serviceProvider.Should().NotBeNull();

			IReadOnlyRepository<Product> productReadOnlyRepository = serviceProvider.GetService<IReadOnlyRepository<Product>>();
			productReadOnlyRepository.Should().BeNull();

			IRepository<Product> productRepository = serviceProvider.GetService<IRepository<Product>>();
			productRepository.Should().NotBeNull().And.BeOfType<ProductRepository>();

			productRepository.Count().Should().Be(127);
		}

		[Fact]
		public void RegisterCustomReadOnlyRepository()
		{
			_services.AddReadOnlyRepository<Product, ProductRepository>();
			ServiceProvider serviceProvider = _services.BuildServiceProvider();
			serviceProvider.Should().NotBeNull();

			IRepository<Product> productRepository = serviceProvider.GetService<IRepository<Product>>();
			productRepository.Should().BeNull();

			IReadOnlyRepository<Product> productReadOnlyRepository = serviceProvider.GetService<IReadOnlyRepository<Product>>();
			productReadOnlyRepository.Should().NotBeNull().And.BeOfType<ProductRepository>();

			productReadOnlyRepository.Count().Should().Be(127);
		}

		[Fact]
		public void RegisterServiceWithWrongLifetime()
		{
			Action addUnitOfWork = () => _services.AddUnitOfWork<TestDbContext>((ServiceLifetime)4);
			Action addRepository = () => _services.AddRepository<Product, ProductRepository>((ServiceLifetime)4);
			Action addReadOnlyRepository = () => _services.AddReadOnlyRepository<Product, ProductRepository>((ServiceLifetime)4);

			addUnitOfWork.Should().Throw<ArgumentOutOfRangeException>();
			addRepository.Should().Throw<ArgumentOutOfRangeException>();
			addReadOnlyRepository.Should().Throw<ArgumentOutOfRangeException>();
		}
	}
}

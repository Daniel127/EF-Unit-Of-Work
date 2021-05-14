using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly DbConnection _dbConnection;
        private readonly IUnitOfWork<TestDbContext> _unitOfWorkProducts;
        private readonly IUnitOfWork<TestDbContext2> _unitOfWorkUsers;

        public UnitOfWorkTests(ITestOutputHelper output)
        {
            _dbConnection = CreateSqLiteInMemoryDatabase();
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<ILogger<IUnitOfWork<TestDbContext>>>(new XUnitLogger<IUnitOfWork<TestDbContext>>(output));
            services.AddSingleton<ILogger<IUnitOfWork<TestDbContext2>>>(new XUnitLogger<IUnitOfWork<TestDbContext2>>(output));

            services.AddDbContext<TestDbContext>(builder =>
            {
                builder.UseInMemoryDatabase("TestUnitOfWork");
            });
            services.AddDbContext<TestDbContext2>(builder =>
            {
                builder.UseSqlite(_dbConnection);
                builder.ConfigureWarnings(configurationBuilder =>
                {
                    configurationBuilder.Ignore(RelationalEventId.AmbientTransactionWarning);
                });
            });

            services.AddSingleton<DbContext>(provider => provider.GetService<TestDbContext>());
            services.AddSingleton<IUnitOfWork<TestDbContext>, UnitOfWork<TestDbContext>>();
            services.AddSingleton<IUnitOfWork<TestDbContext2>, UnitOfWork<TestDbContext2>>();
            services.AddSingleton<IRepository<Product>, ProductRepository>(); //Custom Repository
            services.AddSingleton<IReadOnlyRepository<Product>, ProductRepository>(); //Custom ReadOnlyRepository

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            _unitOfWorkProducts = serviceProvider.GetRequiredService<IUnitOfWork<TestDbContext>>();
            _unitOfWorkUsers = serviceProvider.GetRequiredService<IUnitOfWork<TestDbContext2>>();
            _unitOfWorkUsers.DbContext.Database.EnsureCreated();
        }

        private static DbConnection CreateSqLiteInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _unitOfWorkProducts.DbContext.Products.RemoveRange(_unitOfWorkProducts.DbContext.Products);
            _unitOfWorkProducts.DbContext.SaveChanges();
            _unitOfWorkProducts.Dispose();

            _unitOfWorkUsers.DbContext.Users.RemoveRange(_unitOfWorkUsers.DbContext.Users);
            _unitOfWorkUsers.DbContext.Products.RemoveRange(_unitOfWorkUsers.DbContext.Products);
            _unitOfWorkUsers.DbContext.SaveChanges();
            _unitOfWorkUsers.Dispose();
            _dbConnection.Dispose();
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
        public void GetReadOnlyRepository()
        {
            var userRepository = _unitOfWorkUsers.GetReadOnlyRepository<User>();
            userRepository.Should().NotBeNull();
            userRepository.Any().Should().BeFalse();
        }

        [Fact]
        public void GetReadOnlyRepositoryCreated()
        {
            var userRepository = _unitOfWorkUsers.GetReadOnlyRepository<User>();
            userRepository.Should().NotBeNull();
            userRepository.Any().Should().BeFalse();

            var userRepository2 = _unitOfWorkUsers.GetReadOnlyRepository<User>();
            userRepository2.Should().NotBeNull().And.Be(userRepository);
            userRepository2.Any().Should().BeFalse();
        }

        [Fact]
        public void GetCustomRepository()
        {
            var productRepository = _unitOfWorkProducts.GetRepository<Product>();
            productRepository.Should().NotBeNull().And.BeOfType<ProductRepository>();
            productRepository.Any().Should().BeFalse();
        }

        [Fact]
        public void GetCustomReadOnlyRepository()
        {
            var productRepository = _unitOfWorkProducts.GetReadOnlyRepository<Product>();
            productRepository.Should().NotBeNull().And.BeOfType<ProductRepository>();
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
            var userRepository = _unitOfWorkUsers.GetRepository<User>();

            productRepository.Any().Should().BeFalse();
            productRepository2.Any().Should().BeFalse();
            userRepository.Any().Should().BeFalse();

            await productRepository.InsertAsync(new Product { Name = "Product1", Price = 100 });
            await productRepository2.InsertAsync(new Product { Name = "Product2", Price = 200 });
            await userRepository.InsertAsync(new User { Name = "Test user" });

            productRepository.Any().Should().BeFalse();
            productRepository2.Any().Should().BeFalse();
            userRepository.Any().Should().BeFalse();


            await _unitOfWorkProducts.SaveChangesAsync(new[] { _unitOfWorkUsers });
            productRepository.Any().Should().BeTrue();
            productRepository2.Any().Should().BeTrue();
            userRepository.Any().Should().BeTrue();
        }

        [Fact]
        public void GetFromSqlRaw()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            userRepository.Insert(new User { Id = user1Id, Name = "User 1" });
            userRepository.Insert(new User { Id = user2Id, Name = "User 2" });
            _unitOfWorkUsers.SaveChanges();

            users = _unitOfWorkUsers.FromSqlRaw<User>("SELECT * FROM USERS").ToList();
            users.Should().HaveCount(2);

            users = _unitOfWorkUsers.FromSqlRaw<User>("SELECT * FROM USERS WHERE Id = {0}", user1Id).ToList();
            users.Should()
                .ContainSingle()
                .And.Contain(user => user.Id == user1Id && user.Name == "User 1");
        }

        [Fact]
        public void GetFromSqlInterpolated()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            userRepository.Insert(new User { Id = user1Id, Name = "User 1" });
            userRepository.Insert(new User { Id = user2Id, Name = "User 2" });
            _unitOfWorkUsers.SaveChanges();

            users = _unitOfWorkUsers.FromSqlInterpolated<User>($"SELECT * FROM USERS").ToList();
            users.Should().HaveCount(2);

            users = _unitOfWorkUsers.FromSqlInterpolated<User>($"SELECT * FROM USERS WHERE Id = {user1Id}").ToList();
            users.Should()
                .ContainSingle()
                .And.Contain(user => user.Id == user1Id && user.Name == "User 1");
        }

        [Fact]
        public void ExecuteSqlRaw()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            ICollection<object> parameters = new List<object>
            {
                user1Id, "User 1",
                user2Id, "User 2",
            };
            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            var entitiesModified = _unitOfWorkUsers.ExecuteSqlRaw(
                "INSERT INTO USERS (Id, Name)\nVALUES ({0}, {1}), ({2}, {3})",
                parameters);

            entitiesModified.Should().Be(2);

            users = userRepository.GetAllAsCollection();
            users.Should().HaveCount(2);
            users.Should().Contain(user => user.Id == user1Id && user.Name == "User 1");
            users.Should().Contain(user => user.Id == user2Id && user.Name == "User 2");
        }

        [Fact]
        public async Task ExecuteSqlRawAsync()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            ICollection<object> parameters = new List<object>
            {
                user1Id, "User 1",
                user2Id, "User 2",
            };
            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            var entitiesModified = await _unitOfWorkUsers.ExecuteSqlRawAsync(
                "INSERT INTO USERS (Id, Name)\nVALUES ({0}, {1}), ({2}, {3})",
                parameters);

            entitiesModified.Should().Be(2);

            users = userRepository.GetAllAsCollection();
            users.Should().HaveCount(2);
            users.Should().Contain(user => user.Id == user1Id && user.Name == "User 1");
            users.Should().Contain(user => user.Id == user2Id && user.Name == "User 2");
        }

        [Fact]
        public void ExecuteSqlInterpolated()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            var entitiesModified = _unitOfWorkUsers.ExecuteSqlInterpolated($"INSERT INTO USERS (Id, Name)\nVALUES ({user1Id}, 'User 1'), ({user2Id}, 'User 2')");

            entitiesModified.Should().Be(2);

            users = userRepository.GetAllAsCollection();
            users.Should().HaveCount(2);
            users.Should().Contain(user => user.Id == user1Id && user.Name == "User 1");
            users.Should().Contain(user => user.Id == user2Id && user.Name == "User 2");
        }

        [Fact]
        public async Task ExecuteSqlInterpolatedAsync()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            var userRepository = _unitOfWorkUsers.GetRepository<User>();
            userRepository.Should().NotBeNull();

            var users = userRepository.GetAllAsCollection();
            users.Should().BeEmpty();

            var entitiesModified = await _unitOfWorkUsers.ExecuteSqlInterpolatedAsync($"INSERT INTO USERS (Id, Name)\nVALUES ({user1Id}, 'User 1'), ({user2Id}, 'User 2')");

            entitiesModified.Should().Be(2);

            users = userRepository.GetAllAsCollection();
            users.Should().HaveCount(2);
            users.Should().Contain(user => user.Id == user1Id && user.Name == "User 1");
            users.Should().Contain(user => user.Id == user2Id && user.Name == "User 2");
        }
    }
}

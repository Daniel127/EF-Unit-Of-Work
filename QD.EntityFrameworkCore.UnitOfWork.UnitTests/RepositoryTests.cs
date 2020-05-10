using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections;
using QD.EntityFrameworkCore.UnitOfWork.Collections;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using Xunit;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests
{
	public class RepositoryTests : IDisposable
	{
		private readonly TestDbContext _testDbContext;
		private readonly IRepository<Product> _productsRepository;

		public RepositoryTests()
		{
			DbContextOptionsBuilder<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>();
			options.UseInMemoryDatabase("TestRepository");
			_testDbContext = new TestDbContext(options.Options);
			_productsRepository = new Repository<Product>(_testDbContext);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_testDbContext.RemoveRange(_testDbContext.Products.ToList());
				_testDbContext.SaveChanges();
				_testDbContext.Dispose();
			}
		}

		[Fact]
		public void CreateRepository()
		{
			bool CreateRepository<T>() where T : class
			{
				IRepository<T> repository = new Repository<T>(_testDbContext);
				return repository.Any();
			}

			Func<bool> createRepository = CreateRepository<Product>;
			Func<bool> createRepositoryFake = CreateRepository<ProductFake>;

			createRepository.Should().NotThrow<InvalidOperationException>();
			createRepository().Should().Be(false);
			createRepositoryFake.Should().ThrowExactly<InvalidOperationException>();
		}

		[Fact]
		public void Insert()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			_productsRepository.Insert(product);
			_testDbContext.SaveChanges();
			IEnumerable<Product> allProducts = _productsRepository.GetAll();
			allProducts.Should().NotBeNullOrEmpty().And.Contain(p => p.Id == product.Id).And.HaveCount(1);
		}

		[Fact]
		public void InsertMultiple()
		{
			Product product1 = new Product
			{
				Name = "TestProduct1"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};
			IList<Product> products = new List<Product>
			{
				product1,
				product2
			};
			_productsRepository.Insert(products);
			_testDbContext.SaveChanges();
			IEnumerable<Product> allProducts = _productsRepository.GetAll();
			allProducts
				.Should().NotBeNullOrEmpty()
				.And.HaveCount(2)
				.And.Contain(p => p.Id == product1.Id)
				.And.Contain(p => p.Id == product2.Id);
		}

		[Fact]
		public async Task InsertAsync()
		{
			Product product = new Product
			{
				Name = "TestProductAsync"
			};
			await _productsRepository.InsertAsync(product);
			await _testDbContext.SaveChangesAsync();
			IEnumerable<Product> allProducts = _productsRepository.GetAll();
			allProducts.Should().NotBeNullOrEmpty().And.Contain(p => p.Id == product.Id).And.HaveCount(1);
		}

		[Fact]
		public async Task InsertMultipleAsync()
		{
			Product product1 = new Product
			{
				Name = "TestProduct1"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};
			IList<Product> products = new List<Product>
			{
				product1,
				product2
			};
			await _productsRepository.InsertAsync(products);
			await _testDbContext.SaveChangesAsync();
			IEnumerable<Product> allProducts = _productsRepository.GetAll();
			allProducts
				.Should().NotBeNullOrEmpty()
				.And.HaveCount(2)
				.And.Contain(p => p.Id == product1.Id)
				.And.Contain(p => p.Id == product2.Id);
		}

		[Fact]
		public void GetAll()
		{
			Product productA = new Product
			{
				Name = "TestProductA"
			};
			Product productB = new Product
			{
				Name = "TestProductB"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(productA);
			_productsRepository.Insert(productB);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2).And.OnlyHaveUniqueItems();
		}

		[Fact]
		public void GetAllFiltered()
		{
			Product productA = new Product
			{
				Name = "TestProductA"
			};
			Product productB = new Product
			{
				Name = "TestProductB"
			};

			List<Product> products = _productsRepository.GetAll().ToList();
			products.Should().BeEmpty();

			_productsRepository.Insert(productA);
			_productsRepository.Insert(productB);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll(product => product.Id == productB.Id).ToList();
			products.Should().NotBeNullOrEmpty().And.ContainSingle(product => product.Id == productB.Id);
			products[0].Id.Should().Be(productB.Id);
			products[0].Name.Should().Be("TestProductB");
		}

		[Fact]
		public void GetOne()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_testDbContext.SaveChanges();

			product = _productsRepository.GetFirstOrDefault();
			product.Should().NotBeNull().And.Be(product);

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.ContainSingle().And.Contain(p => p.Id == product.Id).And.ContainSingle();
		}

		[Fact]
		public void GetOneWithPredicate()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			product = _productsRepository.GetFirstOrDefault(p => p.Name == "TestProduct2");
			product.Should().NotBeNull().And.Be(product);

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id)
				.And.Contain(p => p.Id == product2.Id);
		}

		[Fact]
		public async Task GetOneAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_testDbContext.SaveChanges();

			product = await _productsRepository.GetFirstOrDefaultAsync();
			product.Should().NotBeNull().And.Be(product);

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.ContainSingle().And.Contain(p => p.Id == product.Id).And.ContainSingle();
		}

		[Fact]
		public async Task GetOneWithPredicateAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			product = await _productsRepository.GetFirstOrDefaultAsync(p => p.Name == "TestProduct2");
			product.Should().NotBeNull().And.Be(product);

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id)
				.And.Contain(p => p.Id == product2.Id);
		}

		[Fact]
		public void GetOneDto()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_testDbContext.SaveChanges();

			ProductDto productDto = _productsRepository.GetFirstOrDefault(p => new ProductDto { ProductId = p.Id, ProductName = p.Name });

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.ContainSingle()
				.And.Contain(p => p.Id == product.Id);

			productDto.Should().NotBeNull();
			productDto.ProductId.Should().Be(product.Id);
			productDto.ProductName.Should().Be(product.Name);
		}

		[Fact]
		public void GetOneDtoWithPredicate()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			ProductDto productDto = _productsRepository.GetFirstOrDefault(p => new ProductDto { ProductId = p.Id, ProductName = p.Name }, p => p.Name == "TestProduct2");

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id)
				.And.Contain(p => p.Id == product2.Id);

			productDto.Should().NotBeNull();
			productDto.ProductId.Should().Be(product2.Id);
			productDto.ProductName.Should().Be(product2.Name);
		}

		[Fact]
		public async Task GetOneDtoAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_testDbContext.SaveChanges();

			ProductDto productDto = await _productsRepository.GetFirstOrDefaultAsync(p => new ProductDto { ProductId = p.Id, ProductName = p.Name });

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.ContainSingle()
				.And.Contain(p => p.Id == product.Id);

			productDto.Should().NotBeNull();
			productDto.ProductId.Should().Be(product.Id);
			productDto.ProductName.Should().Be(product.Name);
		}

		[Fact]
		public async Task GetOneDtoWithPredicateAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			ProductDto productDto = await _productsRepository.GetFirstOrDefaultAsync(p => new ProductDto { ProductId = p.Id, ProductName = p.Name }, p => p.Name == "TestProduct2");

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id)
				.And.Contain(p => p.Id == product2.Id);

			productDto.Should().NotBeNull();
			productDto.ProductId.Should().Be(product2.Id);
			productDto.ProductName.Should().Be(product2.Name);
		}

		[Fact]
		public void Find()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			Guid productId = product.Id;
			var productFind = _productsRepository.Find(productId);

			productFind.Should().NotBeNull();
			productFind.Id.Should().Be(product.Id);
			productFind.Name.Should().Be(product.Name);
		}

		[Fact]
		public async Task FindAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			Guid productId = product.Id;
			var productFind = await _productsRepository.FindAsync(productId);

			productFind.Should().NotBeNull();
			productFind.Id.Should().Be(product.Id);
			productFind.Name.Should().Be(product.Name);
		}

		[Fact]
		public async Task FindAsyncWithCancellationToken()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			Guid productId = product.Id;
			CancellationToken token = new CancellationToken();
			var productFind = await _productsRepository.FindAsync(new object[] { productId }, token);

			productFind.Should().NotBeNull();
			productFind.Id.Should().Be(product.Id);
			productFind.Name.Should().Be(product.Name);
		}

		[Fact]
		public void UpdateOne()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			product.Price = 500;
			_productsRepository.Update(product);
			_testDbContext.SaveChanges();

			var item = _productsRepository.Find(product.Id);
			item.Should().NotBeNull();
			item.Price.Should().Be(500);
		}

		[Fact]
		public void UpdateMultipleParams()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			product.Price = 500;
			product2.Price = 500;
			_productsRepository.Update(product, product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == 500)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == 500);
		}

		[Fact]
		public void UpdateMultipleEnumerable()
		{
			Product product = new Product
			{
				Name = "TestProduct"
			};
			Product product2 = new Product
			{
				Name = "TestProduct2"
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			product.Price = 500;
			product2.Price = 500;
			IList<Product> productsToUpdate = new List<Product> { product, product2 };
			_productsRepository.Update(productsToUpdate);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == 500)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == 500);
		}

		[Fact]
		public void DeleteOneByKey()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			Guid productId = product.Id;
			_productsRepository.Delete(productId);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty()
				.And.ContainSingle(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);
		}

		[Fact]
		public void DeleteOne()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			_productsRepository.Delete(product);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty()
				.And.ContainSingle(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			Product productToDelete = new Product
			{
				Id = product2.Id
			};

			Action deleteEntity = () => _productsRepository.Delete(productToDelete);
			deleteEntity.Should().ThrowExactly<InvalidOperationException>($"Entity already tracked by {nameof(product2)}");
		}

		[Fact]
		public void DeleteMultiple()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			IQueryable<Product> products = _productsRepository.GetAll();
			products.Should().BeEmpty();

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			IList<Product> productsToDelete = new List<Product> { product, product2 };
			_productsRepository.Delete(productsToDelete);
			_testDbContext.SaveChanges();

			products = _productsRepository.GetAll();
			products.Should().NotBeNull().And.BeEmpty();
		}

		[Fact]
		public void Count()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			_productsRepository.Count().Should().Be(0);

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			var products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			_productsRepository.Count().Should().Be(2);
		}

		[Fact]
		public async Task CountAsync()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			int count = await _productsRepository.CountAsync();
			count.Should().Be(0);

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			var products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			count = await _productsRepository.CountAsync();
			count.Should().Be(2);
		}

		[Fact]
		public void CountWithPredicate()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			_productsRepository.Count().Should().Be(0);

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			var products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			_productsRepository.Count(p => p.Price == 100).Should().Be(1);
			_productsRepository.Count(p => p.Price == 300).Should().Be(0);
		}

		[Fact]
		public async Task CountAsyncWithPredicate()
		{
			Product product = new Product
			{
				Name = "TestProduct",
				Price = 100,
			};
			Product product2 = new Product
			{
				Name = "TestProduct2",
				Price = 200,
			};

			(await _productsRepository.CountAsync()).Should().Be(0);

			_productsRepository.Insert(product);
			_productsRepository.Insert(product2);
			_testDbContext.SaveChanges();

			var products = _productsRepository.GetAll();
			products.Should().NotBeNullOrEmpty().And.HaveCount(2)
				.And.Contain(p => p.Id == product.Id && p.Name == product.Name && p.Price == product.Price)
				.And.Contain(p => p.Id == product2.Id && p.Name == product2.Name && p.Price == product2.Price);

			(await _productsRepository.CountAsync(p => p.Price == 100)).Should().Be(1);
			(await _productsRepository.CountAsync(p => p.Price == 300)).Should().Be(0);
		}

		[Theory]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public void Any(bool insertElement, bool usePredicate)
		{
			if (insertElement)
			{
				Product product = new Product
				{
					Name = "TestProduct",
					Price = 100,
				};

				IQueryable<Product> products = _productsRepository.GetAll();
				products.Should().BeEmpty();

				_productsRepository.Insert(product);
				_testDbContext.SaveChanges();

				if (usePredicate)
				{
					bool result = _productsRepository.Any(p => p.Price == 100);
					result.Should().BeTrue();

					result = _productsRepository.Any(p => p.Price == 300);
					result.Should().BeFalse();
				}
				else
				{
					bool result = _productsRepository.Any();
					result.Should().BeTrue();
				}

			}
			else
			{
				if (usePredicate)
				{
					bool result = _productsRepository.Any(p => p.Price == 100);
					result.Should().BeFalse();
				}
				else
				{
					bool result = _productsRepository.Any();
					result.Should().BeFalse();
				}
			}
		}

		[Fact]
		public void GetPagedList()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i } );
			}

			_productsRepository.Insert(products);
			_testDbContext.SaveChanges();

			IPagedCollection<Product> page1 = _productsRepository.GetPagedList(10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Price}"));
			}

			IPagedCollection<Product> page2 = _productsRepository.GetPagedList(20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<Product> page3 = _productsRepository.GetPagedList(20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Action pageNotFound = () => _productsRepository.GetPagedArray(20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}

		[Fact]
		public async Task GetPagedListAsync()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i });
			}

			await _productsRepository.InsertAsync(products);
			await _testDbContext.SaveChangesAsync();

			IPagedCollection<Product> page1 = await _productsRepository.GetPagedListAsync(10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Price}"));
			}

			IPagedCollection<Product> page2 = await _productsRepository.GetPagedListAsync(20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<Product> page3 = await _productsRepository.GetPagedListAsync(20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedList<Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Func<Task> pageNotFound = async () => await _productsRepository.GetPagedArrayAsync(20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}

		[Fact]
		public void GetPagedArray()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i });
			}

			_productsRepository.Insert(products);
			_testDbContext.SaveChanges();

			IPagedCollection<Product> page1 = _productsRepository.GetPagedArray(10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Price}"));
			}

			IPagedCollection<Product> page2 = _productsRepository.GetPagedArray(20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<Product> page3 = _productsRepository.GetPagedArray(20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Action pageNotFound = () => _productsRepository.GetPagedArray(20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}

		[Fact]
		public async Task GetPagedArrayAsync()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i });
			}

			await _productsRepository.InsertAsync(products);
			await _testDbContext.SaveChangesAsync();

			IPagedCollection<Product> page1 = await _productsRepository.GetPagedArrayAsync(10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Price}"));
			}

			IPagedCollection<Product> page2 = await _productsRepository.GetPagedArrayAsync(20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<Product> page3 = await _productsRepository.GetPagedArrayAsync(20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedArray<Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Func<Task> pageNotFound = async () => await _productsRepository.GetPagedArrayAsync(20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}

		[Fact]
		public void GetPagedDictionary()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i });
			}

			_productsRepository.Insert(products);
			_testDbContext.SaveChanges();

			IPagedCollection<KeyValuePair<Guid, Product>> page1 = _productsRepository.GetPagedDictionary(product => product.Id, 10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Value.Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Value.Price}"));
			}

			IPagedCollection<KeyValuePair<Guid, Product>> page2 = _productsRepository.GetPagedDictionary(product => product.Id, 20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Value.Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<KeyValuePair<Guid, Product>> page3 = _productsRepository.GetPagedDictionary(product => product.Id, 20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Action pageNotFound = () => _productsRepository.GetPagedDictionary(product => product.Id, 20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}

		[Fact]
		public async Task GetPagedDictionaryAsync()
		{
			List<Product> products = new List<Product>();
			for (int i = 0; i < 100; i++)
			{
				products.Add(new Product { Name = $"Product {i}", Price = i });
			}

			await _productsRepository.InsertAsync(products);
			await _testDbContext.SaveChangesAsync();

			IPagedCollection<KeyValuePair<Guid, Product>> page1 = await _productsRepository.GetPagedDictionaryAsync(product => product.Id, 10, orderBy: query => query.OrderBy(product => product.Price));
			page1.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page1.PageSize.Should().Be(10);
			page1.PageNumber.Should().Be(0);
			page1.TotalPages.Should().Be(10);
			page1.HasNextPage.Should().BeTrue();
			page1.HasPreviousPage.Should().BeFalse();
			page1.Items.Should().NotBeNull().And.HaveCount(10);
			for (int i = 0; i < 10; i++)
			{
				page1[i].Value.Price.Should().Be(i, "{0}", page1.Items.Aggregate("", (concat, next) => $"{concat}, {next.Value.Price}"));
			}

			IPagedCollection<KeyValuePair<Guid, Product>> page2 = await _productsRepository.GetPagedDictionaryAsync(product => product.Id, 20, 4, orderBy: query => query.OrderBy(product => product.Price));
			page2.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page2.PageSize.Should().Be(20);
			page2.PageNumber.Should().Be(4);
			page2.TotalPages.Should().Be(5);
			page2.HasNextPage.Should().BeFalse();
			page2.HasPreviousPage.Should().BeTrue();
			page2.Items.Should().NotBeNull().And.HaveCount(20);
			for (int i = 0; i < 20; i++)
			{
				page2[i].Value.Price.Should().Be(20 * 4 + i);
			}

			IPagedCollection<KeyValuePair<Guid, Product>> page3 = await _productsRepository.GetPagedDictionaryAsync(product => product.Id, 20, 0, product => product.Price > 500);
			page3.Should()
				.NotBeNull()
				.And.BeOfType<PagedDictionary<Guid, Product>>();

			page3.PageSize.Should().Be(20);
			page3.PageNumber.Should().Be(0);
			page3.TotalPages.Should().Be(1);
			page3.HasNextPage.Should().BeFalse();
			page3.HasPreviousPage.Should().BeFalse();
			page3.Items.Should().NotBeNull().And.HaveCount(0);

			Func<Task> pageNotFound = async () => await _productsRepository.GetPagedDictionaryAsync(product => product.Id, 20, 1, product => product.Price > 500);
			pageNotFound.Should().Throw<PageNotFoundException>();
		}
	}
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions.Collections;
using QD.EntityFrameworkCore.UnitOfWork.Collections;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Contexts;
using QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests
{
    public class PagedCollectionsTests : IDisposable
    {
        private readonly TestDbContext _testDbContext;
        private readonly IQueryable<Product> _products;
        private const int ProductsNumber = 200;
        private const int MaxPrice = 500;

        public PagedCollectionsTests()
        {
            DbContextOptionsBuilder<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>();
            options.UseInMemoryDatabase("TestPagination");
            _testDbContext = new TestDbContext(options.Options);
            Repository<Product> productsRepository = new Repository<Product>(_testDbContext);

            IList<Product> products = new List<Product>();
            Random random = new Random();
            for (int i = 1; i <= ProductsNumber; i++)
            {
                products.Add(new Product { Id = Guid.NewGuid(), Name = $"Product {i}", Price = random.Next(MaxPrice) });
            }

            productsRepository.Insert(products);
            _testDbContext.SaveChanges();
            _products = productsRepository.GetAll();
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

        public static IList<object[]> ValidationTestData => new List<object[]>
        {
			// pageSize, pageNumber, expectedException
			new object[] { -1, 1, new ArgumentException("Ignored") },
            new object[] { 0, 1, new ArgumentException("Ignored") },
            new object[] { 2, -1, new ArgumentException("Ignored") },
            new object[] { ProductsNumber / 2, -500, new ArgumentException("Ignored") },
            new object[] { 10, (int)Math.Ceiling((double)Math.DivRem(ProductsNumber, 10, out _)) + 1, new PageNotFoundException(-1, -1) },
            new object[] { 5, (int)Math.Ceiling((double)Math.DivRem(ProductsNumber, 5, out _)) + 7, new PageNotFoundException(-1, -1) },
        };

        public static IList<object[]> GetPagedCollectionTestData => new List<object[]>
        {
            new object[] { ProductsNumber, 0, null, false }, // All items
			new object[] { 20, 0, null, false }, // First page with 20 items
			new object[] { 20, 9, null, false }, // Last page with 20 items
			new object[] { 23, 8, null, false }, // Last page with 16 items
			new object[] { 2, 1, (Expression<Func<Product, bool>>)(product => product.Price < 100), false }, // Second page with 2 items and filtered collection
			new object[] { 10, 0, (Expression<Func<Product, bool>>)(product => product.Price > 100), false }, // First page with 10 items and filtered collection
			new object[] { 2, 100, (Expression<Func<Product, bool>>)(product => product.Price <= MaxPrice), true }, // PageNotFoundException
			new object[] { 2, 99, (Expression<Func<Product, bool>>)(product => product.Price <= MaxPrice), false }, // Last page with 2 items
			new object[] { 10, 0, (Expression<Func<Product, bool>>)(product => product.Price <= MaxPrice), false }, // First page with 10 items
			new object[] { 10, 0, (Expression<Func<Product, bool>>)(product => product.Price > MaxPrice), false }, // Page 0 with 0 items
			new object[] { 10, 1, (Expression<Func<Product, bool>>)(product => product.Price > MaxPrice), true }, // PageNotFoundException
			new object[] { 2, 1, (Expression<Func<Product, bool>>)(product => product.Price <= MaxPrice / 2), false }, // Second page with 2 items and filtered collection
			new object[] { 10, 1, (Expression<Func<Product, bool>>)(product => product.Price <= MaxPrice / 2), false }, // Second page with 10 items and filtered collection
			new object[] { 2, 1, (Expression<Func<Product, bool>>)(product => product.Price > MaxPrice / 2), false }, // Second page with 2 items and filtered collection
			new object[] { 10, 1, (Expression<Func<Product, bool>>)(product => product.Price > MaxPrice / 2), false }, // Second page with 10 items and filtered collection
			new object[] { ProductsNumber, 0, (Expression<Func<Product, bool>>)(product => product.Price > MaxPrice / 2), false }, // First page with items < pageSize
		};

        [Theory]
        [MemberData(nameof(ValidationTestData))]
        public void PaginationValidationsMustBeFail<TException>(int pageSize, int pageNumber, TException expectedException) where TException : Exception
        {
            Action getPagedArray = () => _products.ToPagedArray(pageSize, pageNumber);
            Func<Task> getPagedArrayAsync = async () => await _products.ToPagedArrayAsync(pageSize, pageNumber);
            Action getPagedList = () => _products.ToPagedList(pageSize, pageNumber);
            Func<Task> getPagedListAsync = async () => await _products.ToPagedListAsync(pageSize, pageNumber);
            Action getPagedDictionary = () => _products.ToPagedDictionary(product => product.Id, pageSize, pageNumber);
            Func<Task> getPagedDictionaryAsync = async () => await _products.ToPagedDictionaryAsync(product => product.Id, pageSize, pageNumber);

            expectedException.Should().NotBeNull(); // Remove warning, parameter not used

            var exception = getPagedArray.Should().ThrowExactly<TException>().Which;
            getPagedArrayAsync.Should().ThrowExactly<TException>();
            getPagedList.Should().ThrowExactly<TException>();
            getPagedListAsync.Should().ThrowExactly<TException>();
            getPagedDictionary.Should().ThrowExactly<TException>();
            getPagedDictionaryAsync.Should().ThrowExactly<TException>();

            if (exception is not PageNotFoundException pageNotFoundException) return;
            int totalPages = (int)Math.Ceiling((double)Math.DivRem(ProductsNumber, pageSize, out _));
            pageNotFoundException.TotalPages.Should().Be(totalPages);
            pageNotFoundException.PageNumber.Should().Be(pageNumber);
        }

        #region Paged Array
        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public void GetPagedArray(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Action getPagedCollection = () => productsQuery.ToPagedArray(pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<Product> pagedCollection = productsQuery.ToPagedArray(pageSize, pageNumber);
            AssertPagedArray(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public async Task GetPagedArrayAsync(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Func<Task> getPagedCollection = async () => await productsQuery.ToPagedArrayAsync(pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<Product> pagedCollection = await productsQuery.ToPagedArrayAsync(pageSize, pageNumber);
            AssertPagedArray(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        private static void AssertPagedArray(int pageSize, int pageNumber, IPagedCollection<Product> pagedCollection, IQueryable<Product> productsQuery, int totalPages)
        {
            pagedCollection.Should()
                .NotBeNull()
                .And.BeOfType<PagedArray<Product>>();

            int collectionCount = productsQuery.Count();
            pagedCollection.PageSize.Should().Be(pageSize);
            pagedCollection.PageNumber.Should().Be(pageNumber);
            pagedCollection.TotalCount.Should().Be(collectionCount);
            pagedCollection.TotalPages.Should().Be(totalPages);
            pagedCollection.HasPreviousPage.Should().Be(pageNumber > 0);
            pagedCollection.HasNextPage.Should().Be(pageNumber < totalPages - 1);

            int expectedCount;
            if (totalPages == 1 && collectionCount < pageSize)
            {
                expectedCount = collectionCount;
            }
            else if (pageNumber == totalPages - 1)
            {
                expectedCount = collectionCount - pageSize * pageNumber;
            }
            else
            {
                expectedCount = pageSize;
            }
            pagedCollection.Items.Should()
                .HaveCount(expectedCount)
                .And.NotContainNulls()
                .And.AllBeOfType<Product>()
                .And.BeSubsetOf(productsQuery);

            var productsQueryPaged = productsQuery.Skip(pageSize * pageNumber).Take(pageSize).ToList();
            for (int i = 0; i < productsQueryPaged.Count; i++)
            {
                Product product = productsQueryPaged.ElementAt(i);
                pagedCollection[i].Should().Be(product);
                ((PagedArray<Product>)pagedCollection)[i].Should().Be(product);
            }
        }

        #endregion

        #region Paged List
        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public void GetPagedList(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Action getPagedCollection = () => productsQuery.ToPagedList(pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<Product> pagedCollection = productsQuery.ToPagedList(pageSize, pageNumber);
            AssertPagedList(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public async Task GetPagedListAsync(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Func<Task> getPagedCollection = async () => await productsQuery.ToPagedListAsync(pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<Product> pagedCollection = await productsQuery.ToPagedListAsync(pageSize, pageNumber);
            AssertPagedList(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        private static void AssertPagedList(int pageSize, int pageNumber, IPagedCollection<Product> pagedCollection, IQueryable<Product> productsQuery, int totalPages)
        {
            pagedCollection.Should()
                .NotBeNull()
                .And.BeOfType<PagedList<Product>>();

            int collectionCount = productsQuery.Count();
            pagedCollection.PageSize.Should().Be(pageSize);
            pagedCollection.PageNumber.Should().Be(pageNumber);
            pagedCollection.TotalCount.Should().Be(collectionCount);
            pagedCollection.TotalPages.Should().Be(totalPages);
            pagedCollection.HasPreviousPage.Should().Be(pageNumber > 0);
            pagedCollection.HasNextPage.Should().Be(pageNumber < totalPages - 1);

            int expectedCount;
            if (totalPages == 1 && collectionCount < pageSize)
            {
                expectedCount = collectionCount;
            }
            else if (pageNumber == totalPages - 1)
            {
                expectedCount = collectionCount - pageSize * pageNumber;
            }
            else
            {
                expectedCount = pageSize;
            }
            pagedCollection.Items.Should()
                .HaveCount(expectedCount)
                .And.NotContainNulls()
                .And.AllBeOfType<Product>()
                .And.BeSubsetOf(productsQuery);

            var productsQueryPaged = productsQuery.Skip(pageSize * pageNumber).Take(pageSize).ToList();
            for (int i = 0; i < productsQueryPaged.Count; i++)
            {
                Product product = productsQueryPaged.ElementAt(i);
                pagedCollection[i].Should().Be(product);
                ((PagedList<Product>)pagedCollection)[i].Should().Be(product);
            }
        }

        #endregion

        #region Paged Dictionary
        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public void GetPagedDictionary(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Action getPagedCollection = () => productsQuery.ToPagedDictionary(product => product.Id, pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<KeyValuePair<Guid, Product>> pagedCollection = productsQuery.ToPagedDictionary(product => product.Id, pageSize, pageNumber);
            AssertPagedDictionary(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        [Theory]
        [MemberData(nameof(GetPagedCollectionTestData))]
        public async Task GetPagedDictionaryAsync(int pageSize, int pageNumber, Expression<Func<Product, bool>> predicate, bool expectException)
        {
            IQueryable<Product> productsQuery = predicate is null ? _products : _products.Where(predicate);
            int totalPages = (int)Math.Ceiling((double)productsQuery.Count() / pageSize);
            if (totalPages == 0) { totalPages = 1; }

            if (expectException && pageNumber >= totalPages)
            {
                Func<Task> getPagedCollection = async () => await productsQuery.ToPagedDictionaryAsync(product => product.Id, pageSize, pageNumber);
                getPagedCollection.Should().Throw<PageNotFoundException>();
                return;
            }

            IPagedCollection<KeyValuePair<Guid, Product>> pagedCollection = await productsQuery.ToPagedDictionaryAsync(product => product.Id, pageSize, pageNumber);
            AssertPagedDictionary(pageSize, pageNumber, pagedCollection, productsQuery, totalPages);
        }

        private static void AssertPagedDictionary(int pageSize, int pageNumber, IPagedCollection<KeyValuePair<Guid, Product>> pagedCollection, IQueryable<Product> productsQuery, int totalPages)
        {
            pagedCollection.Should()
                .NotBeNull()
                .And.BeOfType<PagedDictionary<Guid, Product>>();

            int collectionCount = productsQuery.Count();
            pagedCollection.PageSize.Should().Be(pageSize);
            pagedCollection.PageNumber.Should().Be(pageNumber);
            pagedCollection.TotalCount.Should().Be(collectionCount);
            pagedCollection.TotalPages.Should().Be(totalPages);
            pagedCollection.HasPreviousPage.Should().Be(pageNumber > 0);
            pagedCollection.HasNextPage.Should().Be(pageNumber < totalPages - 1);

            int expectedCount;
            if (totalPages == 1 && collectionCount < pageSize)
            {
                expectedCount = collectionCount;
            }
            else if (pageNumber == totalPages - 1)
            {
                expectedCount = collectionCount - pageSize * pageNumber;
            }
            else
            {
                expectedCount = pageSize;
            }
            pagedCollection.Items.Should()
                .HaveCount(expectedCount)
                .And.NotContainNulls()
                .And.AllBeOfType<KeyValuePair<Guid, Product>>()
                .And.Subject.Select(pair => pair.Value).Should().BeSubsetOf(productsQuery);

            var productsQueryPaged = productsQuery.Skip(pageSize * pageNumber).Take(pageSize).ToList();
            for (int i = 0; i < productsQueryPaged.Count; i++)
            {
                Product product = productsQueryPaged.ElementAt(i);
                pagedCollection[i].Value.Should().Be(product);
                ((PagedDictionary<Guid, Product>)pagedCollection)[product.Id].Should().Be(product);
            }
        }

        #endregion

        [Fact]
        public void Experiment()
        {
            var pages = new[] {
                _products.ToPagedArray(50),
                _products.ToPagedArray(50, 1),
                _products.ToPagedArray(50, 2),
                _products.ToPagedArray(50, 3),
                _products.Where(product => product.Price > MaxPrice).ToPagedArray(50),
                _products.ToPagedArray(23),
                _products.ToPagedArray(23, 8),
            };
            pages.Should().NotBeNull();
        }
    }
}

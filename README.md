# Status
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=ef-unit-of-work&metric=alert_status)](https://sonarcloud.io/dashboard?id=ef-unit-of-work) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=ef-unit-of-work&metric=ncloc)](https://sonarcloud.io/dashboard?id=ef-unit-of-work) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=ef-unit-of-work&metric=bugs)](https://sonarcloud.io/dashboard?id=ef-unit-of-work) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ef-unit-of-work&metric=coverage)](https://sonarcloud.io/dashboard?id=ef-unit-of-work) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ef-unit-of-work&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=ef-unit-of-work)

[GitHubBadgeMaster]: https://github.com/Daniel127/EF-Unit-Of-Work/workflows/Build/badge.svg?branch=master
[GitHubBadgeDevelop]: https://github.com/Daniel127/EF-Unit-Of-Work/workflows/Build/badge.svg?branch=develop
[GitHubActionsLink]: https://github.com/Daniel127/EF-Unit-Of-Work/actions?query=workflow%3ABuild

[AzureBadgeMaster]: https://dev.azure.com/Daniel127/Entity%20Framework%20-%20Unit%20Of%20Work/_apis/build/status/CI-Release?branchName=master
[AzurePipelineMaster]: https://dev.azure.com/Daniel127/Entity%20Framework%20-%20Unit%20Of%20Work/_build/latest?definitionId=9&branchName=master
[AzureBadgeDevelop]: https://dev.azure.com/Daniel127/Entity%20Framework%20-%20Unit%20Of%20Work/_apis/build/status/CI-Development?branchName=develop
[AzurePipelineDevelop]: https://dev.azure.com/Daniel127/Entity%20Framework%20-%20Unit%20Of%20Work/_build/latest?definitionId=10&branchName=develop

[NugetUrl]: https://www.nuget.org/packages/QD.EntityFrameworkCore.UnitOfWork
[NugetBadge]: https://feeds.dev.azure.com/Daniel127/9d57e78d-f822-418e-ad91-46858d16c35e/_apis/public/Packaging/Feeds/7646e5f2-1d15-485d-98ff-e07b2ae10dd2/Packages/c2532a3a-a889-4da9-b244-567a1ec13fd8/Badge

| Branch | Build | Deployment |
|:----:|:-------------:|:----:|
| master | [![Build][GitHubBadgeMaster]][GitHubActionsLink]  [![Build Status][AzureBadgeMaster]][AzurePipelineMaster] | [![Nuget package][NugetBadge]][NugetUrl] |
| develop | [![Build][GitHubBadgeDevelop]][GitHubActionsLink]  [![Build Status][AzureBadgeDevelop]][AzurePipelineDevelop] | N/A |


# What is it?
Repository and Unit of Work pattern implementation for Entity Framework Core 3.

# How to use?

## Installation
Install the Nuget packages. Use Abstractions for logic layers and the other with insfrastructure layer or monolithic project.

```powershell
dotnet add package QD.EntityFrameworkCore.UnitOfWork
dotnet add package QD.EntityFrameworkCore.UnitOfWork.Abstractions
```

## Register Services

```csharp
public class AppDbContext : DbContext, IDbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    ...
    ...
}
```

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(builder =>
    {
        ...
    });

#region Register UnitOfWorks
    // Register a IUnitOfWork<AppDbContext>
    _services.AddUnitOfWork<AppDbContext>();
    // Register a IUnitOfWork<AppDbContext> and IUnitOfWork
    _services.AddUnitOfWork<AppDbContext>(onlyGeneric: false);
#endregion

    // Optional, register custom repositories
    _services.AddRepository<Product, ProductRepository>();
    _services.AddReadOnlyRepository<Product, ProductReadOnlyRepository>();
}
```

## Use the services

```csharp
public class FancyService : IFancyService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public FancyService(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void DoFancyThing()
    {
        var productsRepository = _unitOfWork.GetRepository<Product>();

        var products = productsRepository.GetAll();

        ...
        // Use inserts, updates, deletes.
        ...

        _unitOfWork.SaveChanges();
    }
}
```

## Paged Collections (v2.0)

```csharp
public void FancyFunction()
{
    // These are some examples

    // Array
    IPagedCollection<Product> page1 = _productsRepository.GetPagedArray(20, 0, product => product.Price > 500);
    IPagedCollection<Product> page2 = await _productsRepository.GetPagedArrayAsync(
        pageSize: 10,
        orderBy: query => query.OrderBy(product => product.Price).ThenBy(product => product.Name)
    );
    // Indexers
    var a = page1[1].Price;
    var b = ((PagedArray<Product>)page1)[2].Price;

    ...

    // List
    IPagedCollection<Product> page1 = _productsRepository.GetPagedList(20, 0, product => product.Price > 500);
    IPagedCollection<Product> page2 = await _productsRepository.GetPagedListAsync(
        pageSize: 10,
        orderBy: query => query.OrderBy(product => product.Price).ThenBy(product => product.Name)
    );
    // Indexers
    var a = page1[1].Price;
    var b = ((PagedList<Product>)page1)[2].Price;

    ...

    // Dictionary
    IPagedCollection<KeyValuePair<Guid, Product>> page1 = _productsRepository.GetPagedDictionary(product => product.Id, 20, 0, product => product.Price > 500);
    IPagedCollection<KeyValuePair<Guid, Product>> page2 = await _productsRepository.GetPagedDictionaryAsync(
        keySelector: product => product.Id,
        pageSize: 10,
        orderBy: query => query.OrderBy(product => product.Price).ThenBy(product => product.Name)
    );
    // Indexers
    var a = page1[1].Value.Price;
    var b = ((PagedDictionary<Guid, Product>)page1)[Guid.Parse("5926d548-c0b3-4c40-b37f-e89be7741024")].Price;
}
```

The before methods are same that

```csharp
    GetAll(predicate, orderBy).ToPagedArray(pageSize, pageNumber);
    GetAll(predicate, orderBy).ToPagedListAsync(pageSize, pageNumber);
    GetAll(predicate, orderBy).ToPagedDictionary(keySelector, pageSize, pageNumber);
```
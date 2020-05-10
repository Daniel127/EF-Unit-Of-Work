using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QD.EntityFrameworkCore.UnitOfWork.Abstractions;
using System;

namespace QD.EntityFrameworkCore.UnitOfWork
{
	/// <summary>
	/// Extension methods for add services.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Registers the unit of work given context as a service in the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <typeparam name="TContext">The type of the db context.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="lifeTime">Specifies the lifetime</param>
		/// <param name="onlyGeneric">If it's <c>true</c> only register <see cref="IUnitOfWork{TContext}"/>, otherwise it also register <see cref="IUnitOfWork"/> with <see cref="UnitOfWork{TContext}"/></param>
		/// <returns>The same service collection so that multiple calls can be chained.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Wrong value of <paramref name="lifeTime"/></exception>
		public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Scoped, bool onlyGeneric = true) where TContext : DbContext, IDbContext
		{
			if (!onlyGeneric)
			{
				services.RegisterService<IUnitOfWork, UnitOfWork<TContext>>(lifeTime);
			}
			return services.RegisterService<IUnitOfWork<TContext>, UnitOfWork<TContext>>(lifeTime);
		}

		/// <summary>
		/// Registers the custom repository of <see cref="IRepository{TEntity}"/> as a service in the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <typeparam name="TRepository">The type of the custom repository.</typeparam>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="lifeTime">Specifies the lifetime</param>
		/// <returns>The same service collection so that multiple calls can be chained.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Wrong value of <paramref name="lifeTime"/></exception>
		public static IServiceCollection AddRepository<TEntity, TRepository>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Scoped) where TEntity : class where TRepository : class, IRepository<TEntity>
		{
			return services.RegisterService<IRepository<TEntity>, TRepository>(lifeTime);
		}

		/// <summary>
		/// Registers the custom read only repository as a service in the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <typeparam name="TRepository">The type of the custom read only repository.</typeparam>
		/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
		/// <param name="lifeTime">Specifies the lifetime</param>
		/// <returns>The same service collection so that multiple calls can be chained.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Wrong value of <paramref name="lifeTime"/></exception>
		public static IServiceCollection AddReadOnlyRepository<TEntity, TRepository>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Scoped) where TEntity : class where TRepository : class, IReadOnlyRepository<TEntity>
		{
			return services.RegisterService<IReadOnlyRepository<TEntity>, TRepository>(lifeTime);
		}

		private static IServiceCollection RegisterService<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Scoped) where TService : class where TImplementation : class, TService
		{
			ServiceDescriptor serviceDescriptor = lifeTime switch
			{
				ServiceLifetime.Singleton => ServiceDescriptor.Singleton<TService, TImplementation>(),
				ServiceLifetime.Scoped => ServiceDescriptor.Scoped<TService, TImplementation>(),
				ServiceLifetime.Transient => ServiceDescriptor.Transient<TService, TImplementation>(),
				_ => throw new ArgumentOutOfRangeException(nameof(lifeTime), lifeTime, null)
			};
			services.Add(serviceDescriptor);
			return services;
		}
	}
}

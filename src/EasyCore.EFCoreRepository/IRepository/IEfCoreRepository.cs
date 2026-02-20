using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// Interface for the repository.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal interface IEfCoreRepository< TEntity> : IRepository<TEntity>, IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
           where TEntity : class, IEntity
    {

    }

    /// <summary>
    /// Interface for the repository.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal interface IEfCoreRepository<TDbContext, TEntity> : IRepository<TDbContext,TEntity>, IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
           where TDbContext : DbContext
           where TEntity : class, IEntity
    {

    }
}

using EasyCore.MongoDbRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.MongoDbRepository.IRepository
{
    /// <summary>
    /// Interface for the repository.
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal interface IMongoDbRepository<TDbContext, TEntity> : IRepository<TDbContext, TEntity>, IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
           where TDbContext : DbContext
           where TEntity : class, IEntity
    {

    }
}

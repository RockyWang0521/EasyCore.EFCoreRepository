using EasyCore.Dependencie;
using EasyCore.MongoDbRepository.DataFilter;
using EasyCore.MongoDbRepository.EntityBase;
using MongoDb.Entity;

namespace EasyCore.MongoDbRepository.Demo.DataFilter
{
    public class CustomDataFilter : IDataFilter, ITransientDependencie
    {
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            query = query.Where(e => ((e as TestEntity)!.Name == "Test"));

            return query;
        }
    }
}

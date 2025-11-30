using EasyCore.Dependencie;
using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.EntityBase;

namespace EasyCore.EFCoreRepository.Demo.DataFilter
{
    public class CustomDataFilter : IDataFilter, ITransientDependencie
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomDataFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
         }

        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            query = query.Where(e => (e as EasyCoreEntity<Guid>)!.CreateTime > DateTime.Now.AddDays(-30));

            return query;
        }
    }
}

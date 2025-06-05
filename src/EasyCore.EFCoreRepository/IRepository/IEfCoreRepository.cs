namespace EasyCore.EFCoreRepository.IRepository
{
    public interface IEfCoreRepository<TEntity> : IRepository<TEntity>, IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
    where TEntity : class
    {

    }
}

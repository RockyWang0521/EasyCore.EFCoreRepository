using EasyCore.EFCoreRepository.EntityBase;

namespace EasyCore.EFCoreRepository.DataFilter
{
    /// <summary>
    /// Interface for data filters.
    /// </summary>
    public interface IDataFilter
    {
        IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity;
    }
}

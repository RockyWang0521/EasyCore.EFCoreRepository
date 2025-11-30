namespace EasyCore.MongoDbRepository.DataFilter
{
    /// <summary>
    /// Interface for tenant filter
    /// </summary>
    public interface ITenantFilter : EasyCore.MongoDbRepository.DataFilter.IDataFilter
    {
        internal string TenantId { get; }
    }
}

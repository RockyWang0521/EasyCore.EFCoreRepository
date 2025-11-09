namespace EasyCore.EFCoreRepository.DataFilter
{
    /// <summary>
    /// Interface for tenant filter
    /// </summary>
    public interface ITenantFilter : EasyCore.EFCoreRepository.DataFilter.IDataFilter
    {
        internal string TenantId { get; }
    }
}

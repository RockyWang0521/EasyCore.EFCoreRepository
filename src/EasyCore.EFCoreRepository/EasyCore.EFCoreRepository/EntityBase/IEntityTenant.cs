namespace EasyCore.EFCoreRepository.EntityBase
{
    /// <summary>
    /// Interface for entity with Tenant.
    /// </summary>
    public interface IEntityTenant
    {
        public string? TenantId { get; set; }
    }
}

namespace EasyCore.MongoDbRepository.EntityBase
{
    /// <summary>
    /// Marker interface for repository entities.
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// Soft-delete support.
    /// </summary>
    public interface IEntitySoftDelete
    {
        bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Multi-tenant support.
    /// </summary>
    public interface IEntityTenant
    {
        string? TenantId { get; set; }
    }

    /// <summary>
    /// Creation timestamp support.
    /// </summary>
    public interface IEntityCreateTime
    {
        DateTime? CreateTime { get; set; }
    }

    /// <summary>
    /// Update timestamp support.
    /// </summary>
    public interface IEntityUpdateTime
    {
        DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// Optimistic concurrency stamp support.
    /// </summary>
    public interface IEntityConcurrencyCheck
    {
        string ConcurrencyStamp { get; set; }
    }
}

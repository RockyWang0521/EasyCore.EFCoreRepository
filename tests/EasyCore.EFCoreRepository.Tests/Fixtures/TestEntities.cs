using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.EFCoreRepository.Tests.Fixtures;

public class SoftDeleteEntity : EasyCoreEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class HardOnlyEntity : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TenantEntity : IEntity, IEntityTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<SoftDeleteEntity> SoftDeleteEntities => Set<SoftDeleteEntity>();
    public DbSet<HardOnlyEntity> HardOnlyEntities => Set<HardOnlyEntity>();
    public DbSet<TenantEntity> TenantEntities => Set<TenantEntity>();
}

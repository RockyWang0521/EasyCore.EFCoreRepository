using EasyCore.EFCoreRepository.EntityBase;
using EasyCore.EFCoreRepository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.Repository
{
    /// <summary>
    /// EFCore仓储基类
    /// 每个方法都加了virtual可以进行重写
    /// </summary>
    /// <typeparam name="TDbContext">DbContext</typeparam>
    /// <typeparam name="TEntity">实体类</typeparam>
    public class EfCoreRepository<TDbContext, TEntity> : BaseEfCoreRepository, IEfCoreRepository<TEntity>
           where TDbContext : DbContext
           where TEntity : class, IEntity
    {
        private readonly TDbContext _dbContext;
        private readonly IServiceProvider? _serviceProvider;

        public EfCoreRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public virtual void Delete([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            var entities = query.Where(predicate).ToList();

            DeleteMany(entities, autoSave);

            if (autoSave)
            {
                dbContext.SaveChanges();
            }
        }

        public virtual void Delete(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            if (entity is IEntitySoftDelete entityEsd)
            {
                entityEsd.IsDeleted = true;
            }
            else
            {
                dbContext.RemoveRange(entity);
            }

            if (autoSave)
            {
                dbContext.SaveChanges();
            }
        }

        public virtual async Task DeleteAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            var entities = query.Where(predicate).ToList();

            await DeleteManyAsync(entities, autoSave, cancellationToken);

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            dbContext.Set<TEntity>().Remove(entity);

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual void DeleteDirect([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            var entities = query.Where(predicate).ToList();

            dbContext.Set<TEntity>().RemoveRange(entities);

            dbContext.SaveChanges();
        }

        public virtual async Task DeleteDirectAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            var entities = query.Where(predicate).ToList();

            dbContext.Set<TEntity>().RemoveRange(entities);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntitySoftDelete[] esdArray)
            {
                foreach (var esd in esdArray) esd.IsDeleted = true;
            }
            else
            {
                dbContext.RemoveRange(entityArray);
            }

            if (autoSave)
            {
                dbContext.SaveChanges();
            }
        }

        public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntitySoftDelete[] esdArray)
            {
                foreach (var esd in esdArray) esd.IsDeleted = true;
            }
            else
            {
                dbContext.RemoveRange(entityArray);
            }

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual TEntity? Get([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            query = ApplyIncludeDetails(query, includeDetails);

            return query.FirstOrDefault();
        }

        public virtual async Task<TEntity> GetAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            query = ApplyIncludeDetails(query, includeDetails);

#pragma warning disable CS8603 
            return await query.FirstOrDefaultAsync(cancellationToken);
#pragma warning restore CS8603 
        }

        public virtual long GetCount(CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return query.Count();
        }

        public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return await query.CountAsync(cancellationToken);
        }

        public virtual long GetCount([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            query.Where(predicate);

            return query.Count();
        }

        public virtual async Task<long> GetCountAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            query.Where(predicate);

            return await query.CountAsync(cancellationToken);
        }

        public virtual List<TEntity> GetList([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return query.ToList();
        }

        public virtual List<TEntity> GetList(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return query.ToList();
        }

        public virtual async Task<List<TEntity>> GetListAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual List<TEntity> GetPagedList(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            if (!string.IsNullOrWhiteSpace(sorting))
            {
                query = ApplySorting(query, sorting);
            }

            return query.Skip(skipCount).Take(maxResultCount).ToList();
        }

        public virtual async Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = ApplyIncludeDetails(query, includeDetails);

            query = TenantDataFilter.ApplyTenantDataFilters<TEntity>(query);

            query = SoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(query);

            if (!string.IsNullOrWhiteSpace(sorting))
            {
                query = ApplySorting(query, sorting);
            }

            return await query.Skip(skipCount).Take(maxResultCount).ToListAsync(cancellationToken);
        }

        private IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sorting)
        {
            var sortingParts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var propertyName = sortingParts[0];
            var isDescending = sortingParts.Length > 1 && sortingParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var propertyInfo = typeof(TEntity).GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(TEntity).Name}'.");
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var keySelector = Expression.Lambda(property, parameter);

            var methodName = isDescending ? "OrderByDescending" : "OrderBy";
            var orderByMethod = typeof(Queryable).GetMethods()
                .First(method => method.Name == methodName && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), propertyInfo.PropertyType);

            return (IQueryable<TEntity>)orderByMethod.Invoke(null, new object[] { query, keySelector })!;
        }

        public virtual TEntity Insert(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var savedEntity = dbContext.Set<TEntity>().Add(entity).Entity;

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (savedEntity is IEntityTenant entityTenant) entityTenant.TenantId = TenantDataFilter.TenantId;

            if (autoSave) dbContext.SaveChanges();

            return savedEntity;
        }

        public async virtual Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var savedEntity = (await dbContext.Set<TEntity>().AddAsync(entity)).Entity;

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (savedEntity is IEntityTenant entityTenant) entityTenant.TenantId = TenantDataFilter.TenantId;

            if (savedEntity is IEntitySoftDelete entitySoft) entitySoft.IsDeleted =false;

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return savedEntity;
        }

        public virtual void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntityTenant[] entityTenant) foreach (var entity in entityTenant) entity.TenantId = TenantDataFilter.TenantId;

            if (entityArray is IEntityConcurrencyCheck[] eruptEntity) foreach (var entity in eruptEntity) entity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (entityArray is IEntitySoftDelete[] entitySoft) foreach (var entity in entitySoft) entity.IsDeleted = false;

            dbContext.Set<TEntity>().AddRange(entityArray);

            if (autoSave)
            {
                dbContext.SaveChanges();
            }
        }

        public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntityTenant[] entityTenant) foreach (var entity in entityTenant) entity.TenantId = TenantDataFilter.TenantId;

            if (entityArray is IEntityConcurrencyCheck[] eruptEntity) foreach (var entity in eruptEntity) entity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (entityArray is IEntitySoftDelete[] entitySoft) foreach (var entity in entitySoft) entity.IsDeleted = false;

            await dbContext.Set<TEntity>().AddRangeAsync(entityArray, cancellationToken);

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public virtual TEntity Update(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            if (dbContext.Set<TEntity>().Local.All(e => e != entity))
            {
                dbContext.Set<TEntity>().Attach(entity);
                dbContext.Update(entity);
            }

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (entity is IEntityTenant entityTenant) entityTenant.TenantId = TenantDataFilter.TenantId;

            if (autoSave)
            {
                dbContext.SaveChanges();
            }

            return entity;
        }

        public async virtual Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            if (dbContext.Set<TEntity>().Local.All(e => e != entity))
            {
                dbContext.Set<TEntity>().Attach(entity);
                dbContext.Update(entity);
            }

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (entity is IEntityTenant entityTenant) entityTenant.TenantId = TenantDataFilter.TenantId;

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity;
        }

        public virtual void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntityTenant[] entityTenant) foreach (var entity in entityTenant) entity.TenantId = TenantDataFilter.TenantId;

            dbContext.Set<TEntity>().UpdateRange(entityArray);

            if (autoSave)
            {
                dbContext.SaveChanges();
            }
        }

        public virtual async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var entityArray = entities.ToArray();

            if (entityArray.Length <= 0)
            {
                return;
            }

            TDbContext dbContext = GetDbContext();

            if (entityArray is IEntityTenant[] entityTenant) foreach (var entity in entityTenant) entity.TenantId = TenantDataFilter.TenantId;

            dbContext.Set<TEntity>().UpdateRange(entityArray);

            if (autoSave)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public int SaveChanges()
        {
            TDbContext dbContext = GetDbContext();

            return dbContext.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            TDbContext dbContext = GetDbContext();

            return await dbContext.SaveChangesAsync();
        }

        private TDbContext GetDbContext() => _serviceProvider == null ? _dbContext : _serviceProvider.GetRequiredService<TDbContext>();

        private IQueryable<TEntity> ApplyIncludeDetails(IQueryable<TEntity> query, bool includeDetails)
        {
            if (!includeDetails) return query;

            var dbContext = GetDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));

            if (entityType == null) return query;

            foreach (var navigation in entityType.GetNavigations())
            {
                query = query.Include(navigation.Name);
            }

            return query;
        }
    }
}
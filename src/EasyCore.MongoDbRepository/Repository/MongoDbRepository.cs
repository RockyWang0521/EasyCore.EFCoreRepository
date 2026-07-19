using EasyCore.MongoDbRepository.DataFilter;
using EasyCore.MongoDbRepository.EntityBase;
using EasyCore.MongoDbRepository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.MongoDbRepository.Repository
{
    /// <summary>
    /// Base repository for entity framework core.
    /// </summary>
    /// <typeparam name="TDbContext">DbContext</typeparam>
    /// <typeparam name="TEntity">Entity</typeparam>
    public class MongoDbRepository<TDbContext, TEntity> :
        BaseMongoDbRepository,
        IMongoDbRepository<TEntity>,
        IMongoDbRepository<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {
        private readonly TDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private List<IDataFilter> _dataFilters = new List<IDataFilter>();

        public MongoDbRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _dataFilters = GetDataFilters(typeof(TEntity));
            _dataFilters = OnApplyPersistingFilters(_dataFilters);
        }

        #region Filter

        public virtual MongoDbRepository<TDbContext, TEntity> AddFilter(Type filterType)
        {
            var filter = base.GetDataFilter(filterType);

            if (filter == null) return this;

            var newList = new List<IDataFilter>(_dataFilters);

            if (!newList.Any(f => f.GetType().FullName == filter.GetType().FullName)) newList.Add(filter);

            var clone = Clone(newList);

            return clone;
        }

        public virtual MongoDbRepository<TDbContext, TEntity> RemoveFilter(Type filterType)
        {
            var filter = base.GetDataFilter(filterType);

            if (filter == null) return this;

            var newList = new List<IDataFilter>(_dataFilters);

            newList.RemoveAll(f => f.GetType().FullName == filter.GetType().FullName);

            var clone = Clone(newList);

            return clone;
        }

        public virtual List<IDataFilter> OnApplyPersistingFilters(List<IDataFilter> dataFilters)
        {
            return dataFilters;
        }

        public List<IDataFilter> AddOnce(List<IDataFilter> dataFilters, Type filterType)
        {
            var filter = base.GetDataFilter(filterType);

            if (!dataFilters.Contains(filter))
            {
                dataFilters.Add(filter);
            }

            return dataFilters;
        }

        public virtual List<IDataFilter> RemoveIfExistsFilter(List<IDataFilter> dataFilters, Type filterType)
        {
            var filter = base.GetDataFilter(filterType);

            dataFilters.RemoveAll(f => f.GetType().FullName == filter.GetType().FullName);

            return dataFilters;
        }

        #endregion

        #region Entity Lifecycle Pre-Processing

        public virtual void OnBeforeAdd(TEntity entity)
        {
        }

        public virtual void OnBeforeUpdate(TEntity entity)
        {
        }

        public virtual void OnBeforeDelete(TEntity entity)
        {
        }

        #endregion

        #region Delete

        public virtual void Delete([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            var entities = query.ToList();

            DeleteMany(entities, autoSave);
        }

        public virtual void Delete(TEntity entity, bool autoSave = false)
        {
            ArgumentNullException.ThrowIfNull(entity);

            TDbContext dbContext = GetDbContext();

            ApplyDelete(dbContext, entity);

            if (autoSave) dbContext.SaveChanges();
        }

        public virtual async Task DeleteAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            var entities = await query.ToListAsync(cancellationToken);

            await DeleteManyAsync(entities, autoSave, cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);

            TDbContext dbContext = GetDbContext();

            ApplyDelete(dbContext, entity);

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void DeleteDirect([NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            // Bypass soft-delete / tenant filters for true hard delete.
            var entities = dbContext.Set<TEntity>().AsQueryable().Where(predicate).ToList();

            dbContext.Set<TEntity>().RemoveRange(entities);

            dbContext.SaveChanges();
        }

        public virtual async Task DeleteDirectAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            // Bypass soft-delete / tenant filters for true hard delete.
            var entities = await dbContext.Set<TEntity>().AsQueryable().Where(predicate).ToListAsync(cancellationToken);

            dbContext.Set<TEntity>().RemoveRange(entities);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void DeleteManyDirect(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();

            dbContext.Set<TEntity>().RemoveRange(entityArray);

            if (autoSave) dbContext.SaveChanges();
        }

        public virtual async Task DeleteManyDirectAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();

            dbContext.Set<TEntity>().RemoveRange(entityArray);

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();

            foreach (var entity in entityArray)
            {
                ApplyDelete(dbContext, entity);
            }

            if (autoSave) dbContext.SaveChanges();
        }

        public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();

            foreach (var entity in entityArray)
            {
                ApplyDelete(dbContext, entity);
            }

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Get

        #region GetEnumerable

        public virtual TEntity? GetFirst([NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.FirstOrDefault();
        }

        public virtual async Task<TEntity> GetFirstAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.FirstAsync(cancellationToken);
        }

        public virtual async Task<TEntity?> GetFirstOrDefaultAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual List<TEntity> GetList([NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.ToList();
        }

        public virtual List<TEntity> GetList()
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.ToList();
        }

        public virtual async Task<List<TEntity>> GetListAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.ToListAsync(cancellationToken);
        }

        public virtual List<TEntity> GetPagedList(int skipCount, int maxResultCount)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.Skip(skipCount).Take(maxResultCount).ToList();
        }

        public virtual List<TEntity> GetPagedList(int skipCount, int maxResultCount, [NotNull] Expression<Func<TEntity, object>> orderBy, bool ascending = true)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return query.Skip(skipCount).Take(maxResultCount).ToList();
        }

        public virtual async Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.Skip(skipCount).Take(maxResultCount).ToListAsync(cancellationToken);
        }

        public virtual async Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, [NotNull] Expression<Func<TEntity, object>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return await query.Skip(skipCount).Take(maxResultCount).ToListAsync(cancellationToken);
        }

        public virtual List<TEntity> GetPagedList(int skipCount, int maxResultCount, [NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.Skip(skipCount).Take(maxResultCount).ToList();
        }

        public virtual async Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, [NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.Skip(skipCount).Take(maxResultCount).ToListAsync(cancellationToken);
        }

        #endregion

        #region GetQueryable

        public virtual IQueryable<TEntity> AsQueryable([NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().Where(predicate);

            return _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));
        }

        public virtual IQueryable<TEntity> AsQueryable()
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            return _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));
        }

        public virtual IQueryable<TEntity> GetPageQuery(int skipCount, int maxResultCount, [NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.Skip(skipCount).Take(maxResultCount);
        }

        #endregion

        #endregion

        #region Count

        public virtual long GetCount()
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.Count();
        }

        public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable();

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.CountAsync(cancellationToken);
        }

        public virtual long GetCount([NotNull] Expression<Func<TEntity, bool>> predicate)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return query.Count();
        }

        public virtual async Task<long> GetCountAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var query = dbContext.Set<TEntity>().AsQueryable().Where(predicate);

            query = _dataFilters.Aggregate(query, (current, filter) => filter.Apply(current));

            return await query.CountAsync(cancellationToken);
        }

        #endregion

        #region DbSet

        public virtual DbSet<TEntity> EntityDbSet()
        {
            TDbContext dbContext = GetDbContext();

            return dbContext.Set<TEntity>();
        }

        #endregion

        #region Insert

        public virtual TEntity Insert(TEntity entity, bool autoSave = false)
        {
            TDbContext dbContext = GetDbContext();

            var savedEntity = dbContext.Set<TEntity>().Add(entity).Entity;

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (savedEntity is IEntityTenant entityTenant) ApplyTenantId(entityTenant);

            if (savedEntity is IEntitySoftDelete entitySoft) entitySoft.IsDeleted = false;

            if (savedEntity is IEntityCreateTime createTime) createTime.CreateTime = DateTime.Now;

            OnBeforeAdd(entity);

            if (autoSave) dbContext.SaveChanges();

            return savedEntity;
        }

        public async virtual Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            var savedEntity = (await dbContext.Set<TEntity>().AddAsync(entity)).Entity;

            if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (savedEntity is IEntityTenant entityTenant) ApplyTenantId(entityTenant);

            if (savedEntity is IEntitySoftDelete entitySoft) entitySoft.IsDeleted = false;

            if (savedEntity is IEntityCreateTime createTime) createTime.CreateTime = DateTime.Now;

            OnBeforeAdd(entity);

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);

            return savedEntity;
        }

        public virtual void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            if (entities.Count() <= 0) return;

            TDbContext dbContext = GetDbContext();

            var now = DateTime.Now;

            foreach (var entity in entities)
            {
                if (entity is IEntityTenant tenant) ApplyTenantId(tenant);

                if (entity is IEntityConcurrencyCheck concurrency) concurrency.ConcurrencyStamp = Guid.NewGuid().ToString();

                if (entity is IEntitySoftDelete soft) soft.IsDeleted = false;

                if (entity is IEntityCreateTime ct) ct.CreateTime = now;

                OnBeforeAdd(entity);
            }

            dbContext.Set<TEntity>().AddRange(entities);

            if (autoSave) dbContext.SaveChanges();
        }

        public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            if (entities.Count() <= 0) return;

            TDbContext dbContext = GetDbContext();

            var now = DateTime.Now;

            foreach (var entity in entities)
            {
                if (entity is IEntityTenant tenant) ApplyTenantId(tenant);

                if (entity is IEntityConcurrencyCheck concurrency) concurrency.ConcurrencyStamp = Guid.NewGuid().ToString();

                if (entity is IEntitySoftDelete soft) soft.IsDeleted = false;

                if (entity is IEntityCreateTime ct) ct.CreateTime = now;

                OnBeforeAdd(entity);
            }

            await dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Update

        public virtual TEntity Update(TEntity entity, bool autoSave = false)
        {
            TDbContext dbContext = GetDbContext();

            if (dbContext.Set<TEntity>().Local.All(e => e != entity))
            {
                dbContext.Set<TEntity>().Attach(entity);

                dbContext.Update(entity);
            }

            if (dbContext.Entry(entity).State == EntityState.Modified)
            {
                if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

                if (entity is IEntityUpdateTime entityUpdateTime) entityUpdateTime.UpdateTime = DateTime.Now;
            }

            if (entity is IEntityTenant entityTenant) ApplyTenantId(entityTenant);

            OnBeforeUpdate(entity);

            if (autoSave) dbContext.SaveChanges();

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

            if (dbContext.Entry(entity).State == EntityState.Modified)
            {
                if (entity is IEntityConcurrencyCheck eruptEntity) eruptEntity.ConcurrencyStamp = Guid.NewGuid().ToString();

                if (entity is IEntityUpdateTime entityUpdateTime) entityUpdateTime.UpdateTime = DateTime.Now;
            }

            if (entity is IEntityTenant entityTenant) ApplyTenantId(entityTenant);

            OnBeforeUpdate(entity);

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public virtual void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();
            var now = DateTime.Now;

            foreach (var entity in entityArray)
            {
                ApplyUpdateStamps(dbContext, entity, now);
            }

            if (autoSave) dbContext.SaveChanges();
        }

        public virtual async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityArray = entities as TEntity[] ?? entities.ToArray();

            if (entityArray.Length == 0) return;

            TDbContext dbContext = GetDbContext();
            var now = DateTime.Now;

            foreach (var entity in entityArray)
            {
                ApplyUpdateStamps(dbContext, entity, now);
            }

            if (autoSave) await dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region  Aiding Method

        public int SaveChanges()
        {
            TDbContext dbContext = GetDbContext();

            return dbContext.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TDbContext dbContext = GetDbContext();

            return await dbContext.SaveChangesAsync(cancellationToken);
        }

        public TDbContext GetDbContext() => _serviceProvider == null ? _dbContext : _serviceProvider.GetRequiredService<TDbContext>();

        /// <summary>
        /// Soft-delete or hard-delete a single entity, then invoke <see cref="OnBeforeDelete"/>.
        /// </summary>
        protected virtual void ApplyDelete(TDbContext dbContext, TEntity entity)
        {
            if (entity is IEntitySoftDelete softDelete)
            {
                softDelete.IsDeleted = true;

                if (dbContext.Set<TEntity>().Local.All(e => e != entity))
                {
                    dbContext.Update(entity);
                }

                OnBeforeDelete(entity);
                return;
            }

            dbContext.Set<TEntity>().Remove(entity);
            OnBeforeDelete(entity);
        }

        /// <summary>
        /// Attach/update stamps for bulk update, then invoke <see cref="OnBeforeUpdate"/>.
        /// </summary>
        protected virtual void ApplyUpdateStamps(TDbContext dbContext, TEntity entity, DateTime now)
        {
            if (dbContext.Set<TEntity>().Local.All(e => e != entity))
            {
                dbContext.Set<TEntity>().Attach(entity);
                dbContext.Update(entity);
            }

            if (entity is IEntityConcurrencyCheck concurrency)
                concurrency.ConcurrencyStamp = Guid.NewGuid().ToString();

            if (entity is IEntityTenant entityTenant)
                ApplyTenantId(entityTenant);

            if (entity is IEntityUpdateTime ct)
                ct.UpdateTime = now;

            OnBeforeUpdate(entity);
        }

        /// <summary>
        /// Stamps tenant id from the current provider. Uses null when no tenant is available (never empty string).
        /// </summary>
        protected virtual void ApplyTenantId(IEntityTenant entity)
        {
            var tenantId = TenantFilter.TenantId;
            entity.TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
        }

        private List<IDataFilter> GetDataFilters(Type entityType)
        {
            var dataFilters = new List<IDataFilter>();

            if (typeof(IEntitySoftDelete).IsAssignableFrom(entityType))
                dataFilters.Add(SoftDeleteFilter);

            if (typeof(IEntityTenant).IsAssignableFrom(entityType))
                dataFilters.Add(TenantFilter);

            return dataFilters;
        }

        /// <summary>
        /// Clones this repository with a new filter list while preserving the runtime type,
        /// so overrides of OnBeforeAdd/Update/Delete on derived repositories remain effective.
        /// </summary>
        protected virtual MongoDbRepository<TDbContext, TEntity> CreateClone(List<IDataFilter> dataFilters)
        {
            var clone = (MongoDbRepository<TDbContext, TEntity>)MemberwiseClone();
            clone._dataFilters = new List<IDataFilter>(dataFilters);
            return clone;
        }

        private MongoDbRepository<TDbContext, TEntity> Clone(List<IDataFilter> dataFilters) => CreateClone(dataFilters);

        protected MongoDbRepository(TDbContext dbContext, IServiceProvider serviceProvider, List<IDataFilter> dataFilters) : base(serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _dataFilters = dataFilters;
        }

        public List<IDataFilter> DataFilters => _dataFilters;

        #endregion
    }
}
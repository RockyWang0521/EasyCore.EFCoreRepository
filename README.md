# 🏗️ EasyCore.EFCoreRepository

[English README](https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/README.en-US.md)  |  [MongoDb English README](https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/README.MongoDb.en-US.md)

📚 概述
Repository 是软件开发中的一个重要概念，尤其在 领域驱动设计（DDD） 🎯 和 数据访问层 中广泛使用。Repository 是一种 抽象数据访问层的设计模式 🏛️，它封装了数据访问逻辑，使上层业务逻辑与底层数据存储解耦。Repository 就像是数据仓库的接口 📦，用于管理实体对象的持久化（增删改查）操作。

## 🚀 快速开始
### 1. 📝 Program 注册

```
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.EasyCoreDependencie();

        builder.Services.AddDbContext<TestDbContext>();

        // ✨ 使用 EasyCore EFCore Repository
        builder.Services.EasyCoreEFCoreRepository();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
```
### 2. 🏷️ 实体继承
EasyCore.EFCoreRepository 提供了一个功能丰富的实体基类 EasyCoreEntity，包含：

   🔄 并发标记

   🗑️ 软删除

   🏢 多租户 ID

```
// 💡 注意：EasyCoreEntity<TKey> 中的泛型类型为表的主键类型
public class TestEntity : EasyCoreEntity<Guid>
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

### 3. 🔧 仓储类继承
EasyCore.EFCoreRepository 提供了完整的仓储抽象和实现：

#### 仓储接口 📜
```
public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
{
    // 💡 ITransientDependencie 为 EasyCore.Dependencie 中的自动注入标记
}
```

#### 仓储实现 ⚙️
```
public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
{
    public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) 
        : base(dbContext, serviceProvider)
    {
    }
}
```
### 4.💡 使用仓储

#### 4.1 🎯 基础 CRUD 操作
```
[Route("api/[controller]")]
[ApiController]
public class RepositoryController : ControllerBase
{
    private readonly ITestEntityRepository _repository;

    public RepositoryController(ITestEntityRepository repository) => _repository = repository;

    // 🔍 查询
    [HttpGet]
    public async Task<TestEntity> Get()
    {
        return await _repository.GetAsync(e => e.Name == "Test");
    }

    // ➕ 新增
    [HttpPost]
    public async Task Post()
    {
        await _repository.InsertAsync(new TestEntity { 
            Name = "Test", 
            Age = 10, 
            Id = Guid.NewGuid() 
        }, true);
    }

    // ✏️ 更新
    [HttpPut]
    public async Task Put()
    {
        var entity = await _repository.GetAsync(e => e.Name == "Test");
        entity.Age = 20;
        await _repository.UpdateAsync(entity, true);
    }

    // 🗑️ 删除（软删除）
    [HttpDelete]
    public async Task Delete()
    {
        var entity = await _repository.GetAsync(e => e.Age == 20);
        entity.IsDeleted = true;
        await _repository.UpdateAsync(entity, true);
    }
}
```

#### 4.2 📋 完整的 API 列表

EasyCore.EFCoreRepository 提供了丰富的 API 方法：

##### 🔧 过滤器管理
```
EfCoreRepository<TDbContext, TEntity> AddFilter(Type filterType);

EfCoreRepository<TDbContext, TEntity> RemoveFilter(Type filterType);
```
##### ➕ 插入操作
```
Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

TEntity Insert(TEntity entity, bool autoSave = false);

Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### ✏️ 更新操作
```
Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

TEntity Update(TEntity entity, bool autoSave = false);

Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### 🗑️ 删除操作
```
Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

void Delete(TEntity entity, bool autoSave = false);

Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### 💾 保存操作
```
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

int SaveChanges();
```
##### 🔍 查询操作
```
// 获取列表
Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

List<TEntity> GetList(bool includeDetails = false);

// 数量统计
Task<long> GetCountAsync(CancellationToken cancellationToken = default);

long GetCount();

Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

long GetCount(Expression<Func<TEntity, bool>> predicate);

// 分页查询
Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false, CancellationToken cancellationToken = default);

List<TEntity> GetPagedList(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false);

// 条件查询
Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default);

List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false);

// 单实体查询
Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default);

TEntity? Get(Expression<Func<TEntity, bool>> predicate, bool includeDetails = false);
```
##### ⚡ 直接删除操作
```
Task DeleteDirectAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

void DeleteDirect(Expression<Func<TEntity, bool>> predicate);

void DeleteManyDirect(IEnumerable<TEntity> entities, bool autoSave = false);

Task DeleteManyDirectAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
```
### 5.🎛️ 高级功能
#### 🎯 WhereIf 支持
EasyCore.EFCoreRepository 提供了智能的条件查询支持：
```
IQueryable<T>.WhereIf(xxx != null, x => x.xxx == xxx)
```
✨ 特性：只有当 xxx != null 条件满足时，才会执行后面的过滤条件，否则继续执行后续代码。

### 6. 🔍 数据过滤器
EasyCore.EFCoreRepository 内置了两个实用的数据过滤器：

🗑️ ISoftDeleteFilter - 软删除过滤器

🏢 ITenantFilter - 租户过滤器

#### 自定义过滤器示例 🎨：
```
public class CustomDataFilter : IDataFilter, ITransientDependencie
{
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
    {
        query = query.Where(e => ((e as TestEntity)!.Name == "Test"));
        return query;
    }
}
```
#### 动态过滤器管理 ⚡：
```
_repository
    .RemoveFilter(typeof(ITenantFilter))      // 🗑️ 移除租户过滤器
    .RemoveFilter(typeof(ISoftDeleteFilter))  // 🗑️ 移除软删除过滤器  
    .AddFilter(typeof(CustomDataFilter))      // ➕ 添加自定义过滤器
    .Delete(e => e.Name == "Test1", true);    // 🎯 执行操作
```

# 🔄 EasyCore.UnitOfWork

## 🎯 工作单元模式

EasyCore.UnitOfWork 提供了 SaveChangesAttribute 特性，让数据持久化变得简单高效！✨

### 1. 📝 Program 注册

```
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.EasyCoreDependencie();
        builder.Services.AddDbContext<TestDbContext>();

        // ✨ 使用 EasyCore EFCore Repository
        builder.Services.EasyCoreEFCoreRepository();
        // 🔄 使用 EasyCore EFCore UnitOfWork
        builder.Services.EasyCoreUnitOfWork();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
```

### 2. 📜 抽象接口定义

```
public interface IUnitOfWorkTest : ITransientDependencie
{
    /// <summary>
    /// 🎯 测试实体工作单元
    /// </summary>
    Task<TestEntity> EntityUnitOfWork();

    /// <summary>
    /// 💰 测试事务工作单元  
    /// </summary>
    Task<TestEntity> Transaction();
}

public interface IUnitOfWorkTest2 : ITransientDependencie
{
    /// <summary>
    /// 🎯 测试实体工作单元
    /// </summary>
    Task<TestEntity> EntityUnitOfWork();
}
```

### 3. 🏷️ SaveChangesAttribute 特性使用

#### 方法级别使用 🎯：

```
public class UnitOfWorkTest : IUnitOfWorkTest
{
    private readonly ITestEntityRepository _repository;

    public UnitOfWorkTest(ITestEntityRepository repository) => _repository = repository;

    [SaveChanges(typeof(TestDbContext))]
    public Task<TestEntity> EntityUnitOfWork() 
        => _repository.InsertAsync(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });

    [SaveChanges(true, typeof(TestDbContext))]  // 💰 启用事务
    public Task<TestEntity> Transaction() 
        => _repository.InsertAsync(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });
}
```

#### 类级别使用 🏛️：

```
[SaveChanges(typeof(TestDbContext))]
public class UnitOfWorkTest2 : IUnitOfWorkTest2
{
    private readonly ITestEntityRepository _repository;

    public UnitOfWorkTest2(ITestEntityRepository repository) => _repository = repository;

    public Task<TestEntity> EntityUnitOfWork() 
        => _repository.InsertAsync(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });
}
```

#### 💡 特性参数说明：

第一个参数：是否为数据库事务，为 true 时执行事务保存 💰

第二个参数：指定要保存的数据库 DbContext 对象

# 🔍 EasyCore.EntityChange
## 📊 实体变更追踪
EasyCore.EntityChange 提供了强大的实体变更追踪能力！🕵️

### 1. 📝 Program 注册

```
builder.Services.AddDbContext<TestDbContext>(op =>
{
    op.UseEasyCoreEntityChange(builder.Services); // ✨ 使用 EasyCore EFCore 实体变更追踪
});

builder.Services.AddDbContext<Test2DbContext>(op =>
{
    op.UseEasyCoreEntityChange(builder.Services); // ✨ 使用 EasyCore EFCore 实体变更追踪
});

// 🔧 启用 EasyCore 实体变更服务
builder.Services.EasyCoreEntityChange();
```

### 2. 🎯 使用实体变更追踪

```
public class EntityChange : 
    IEntityUpdatedChangeHandler<TestEntity, TestEntity>, 
    IEntityDeletedChangeHandler<TestEntity>, 
    IEntityAddedChangeHandler<TestEntity>
{
    private readonly ILogger<EntityChange> _logger;

    public EntityChange(ILogger<EntityChange> logger) => _logger = logger;

    // ➕ 实体新增处理
    public async Task OnAddedAsync(TestEntity entity)
    {
        _logger.LogInformation($"🆕 实体新增: Id:{entity.Id}; Name:{entity.Name}; Age:{entity.Age};");
        await Task.CompletedTask;
    }

    // 🗑️ 实体删除处理  
    public async Task OnDeletedAsync(TestEntity entity)
    {
        _logger.LogInformation($"🗑️ 实体删除: Id:{entity.Id}; Name:{entity.Name}; Age:{entity.Age};");
        await Task.CompletedTask;
    }

    // ✏️ 实体更新处理
    public Task OnUpdatedAsync(TestEntity oldEntity, TestEntity currentEntity)
    {
        _logger.LogInformation($"✏️ 实体更新: " +
            $"Id:{oldEntity.Id} → {currentEntity.Id}; " +
            $"Name:{oldEntity.Name} → {currentEntity.Name}; " +
            $"Age:{oldEntity.Age} → {currentEntity.Age};");
        return Task.CompletedTask;
    }
}
```

#### 🎯 支持的变更接口：

```
IEntityAddedChangeHandler<TEntity> - 实体新增处理器 ➕

IEntityDeletedChangeHandler<TEntity> - 实体删除处理器 🗑️

IEntityUpdatedChangeHandler<TOriginalEntity, TCurrentEntity> - 实体更新处理器 ✏️
```
# 💎 Custom Entity 和 Custom Data Filter

## 1.  用户自定义数据库实体🦄

用户可根据自身项目进行自定义用户实体配置

```
    public class CustomEntity : EasyCoreEntity<Guid>
    {
        public string CreateId{ get; set; }
    }

    public class TestCustomEntity : CustomEntity
    {

    }
```
CustomEntity为用户自定义实体对象，自定义实体中存在CreateId字段。保存时可用IEntityAddedChangeHandler<TEntity> 自动保存当前用户id。

```
    public class TestCustomEntityRepository : EfCoreRepository<TestDbContext, TestCustomEntity>,ITestCustomEntityRepository,IEntityAddedChangeHandler<TestCustomEntity>
    {
        public TestCustomEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }

        public async Task OnAddedAsync(TestCustomEntity entity)
        {
            if (entity is CustomEntity customEntity)
            {
                customEntity.CreateId = "Test";
            }

            await Task.CompletedTask;
        }
    }
```
## 2.  用户自定义数据过滤器🎁

用户可根据自身项目进行自定义数据过滤器配置


```
    public class TestEntityRepository :
        EfCoreRepository<TestDbContext, TestEntity>,
        ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }

        /// <summary>
        /// Applies permanent data filters before persisting entities (Insert/Update/Delete).
        /// This method is called during the persistence pipeline to enforce global filters.
        /// </summary>
        /// <param name="dataFilters">The list of data filters that are currently scheduled for execution.</param>
        /// <returns>The updated list of data filters after applying permanent filter rules.</returns>
        public override List<IDataFilter> OnApplyPersistingFilters(List<IDataFilter> dataFilters)
        {
            AddOnce(dataFilters, typeof(CustomDataFilter));

            RemoveIfExistsFilter(dataFilters, typeof(CustomDataFilter));

            return dataFilters;
        }
    }
```
重写OnApplyPersistingFilters方法，添加或修改永久过滤器设置。


#### ✨ 特性：当实体完成增删改操作时，系统会自动调用对应的接口方法，实现无缝的变更追踪！

# 🎉 总结
EasyCore.EFCoreRepository 系列组件提供了：

🏗️ 完整的仓储模式实现

🔄 智能的工作单元管理

🔍 强大的实体变更追踪

🎯 丰富的查询和过滤功能

⚡ 高性能的数据访问

让您的数据访问层更加 优雅、强大、易维护！✨
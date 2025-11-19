# 🏗️ EasyCore.MongoDbRepository

[English README] (https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/%20README.en-US.md)  |  [MongoDb English README](https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/%20README.en-US.md)

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
        builder.Services.EasyCoreMongoDbRepository();

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
EasyCore.MongoDbRepository 提供了一个功能丰富的实体基类 EasyCoreEntity，包含：

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
EasyCore.MongoDbRepository 提供了完整的仓储抽象和实现：

#### 仓储接口 📜
```
public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
{
    // 💡 ITransientDependencie 为 EasyCore.Dependencie 中的自动注入标记
}
```

#### 仓储实现 ⚙️
```
public class TestEntityRepository : MongoDbRepository<TestDbContext, TestEntity>, ITestEntityRepository
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
        await _repository.InsertAsync(new TestEntity
        {
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

EasyCore.MongoDbRepository 提供了丰富的 API 方法：

##### 🔧 过滤器管理
```
MongoDbRepository<TDbContext, TEntity> AddFilter(Type filterType);

MongoDbRepository<TDbContext, TEntity> RemoveFilter(Type filterType);
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
EasyCore.MongoDbRepository 提供了智能的条件查询支持：
```
IQueryable<T>.WhereIf(xxx != null, x => x.xxx == xxx)
```
✨ 特性：只有当 xxx != null 条件满足时，才会执行后面的过滤条件，否则继续执行后续代码。

### 6. 🔍 数据过滤器
EasyCore.MongoDbRepository 内置了两个实用的数据过滤器：

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


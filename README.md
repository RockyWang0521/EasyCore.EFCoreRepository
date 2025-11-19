# EasyCore.EFCoreRepository


Repository 是软件开发中的一个重要概念，尤其在 领域驱动设计（DDD） 和 数据访问层 中广泛使用。Repository 是一种 抽象数据访问层的设计模式，它封装了数据访问逻辑，使上层业务逻辑与底层数据存储解耦。Repository 就像是数据仓库的接口，用于管理实体对象的持久化（增删改查）操作。

## 1.Program注册

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

         // Use EasyCore EFCore Repository
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

## 2.实体继承

EasyCore.EFCoreRepository 提供了一个实体基类 EasyCoreEntity，包含并发标记/软删除/以及多租户ID。

```
    // 注：EasyCoreEntity<TKey>中的泛型类型输入为表的主键。
    public class TestEntity : EasyCoreEntity<Guid>
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
```

## 3.仓储类继承

EasyCore.EFCoreRepository 提供了一个仓储基类 EfCoreRepository<TDbContext, TEntity> 和仓储抽象 IRepository<TEntity>。


```
    public interface ITestEntityRepository : IRepository<TestDbContext，TestEntity>, ITransientDependencie
    {

    }
```
（注：ITransientDependencie为EasyCore.Dependencie中的自动注入细节与抽象的依赖关系）

```
  public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
  {
      public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
      {

      }
  }
```

## 4.使用仓储

### 4.1 增删改查
```
 [Route("api/[controller]")]
 [ApiController]
 public class RepositoryController : ControllerBase
 {
     private readonly ITestEntityRepository _repository;

     public RepositoryController(ITestEntityRepository repository) => _repository = repository;

     [HttpGet]
     public async Task<TestEntity> Get()
     {
         return await _repository.GetAsync(e => e.Name == "Test");
     }

     [HttpPost]
     public async Task Post()
     {
         await _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Guid = Guid.NewGuid() }, true);
     }

     [HttpPut]
     public async Task Put()
     {
         var entity = await _repository.GetAsync(e => e.Name == "Test");

         entity.Age = 20;

         await _repository.UpdateAsync(entity, true);
     }

     [HttpDelete]
     public async Task Delete()
     {
         var entity = await _repository.GetAsync(e => e.Age == 20);

         entity.IsDeleted = true;

         await _repository.UpdateAsync(entity, true);
     }
 }
```
### 4.2 仓储提供的api

```
/// <summary>
/// 向仓储添加指定类型的过滤器。
/// </summary>
/// <param name="filterType">要添加的过滤器类型。</param>
/// <returns>应用了过滤器的仓储实例。</returns>
EfCoreRepository<TDbContext, TEntity> AddFilter(
    Type filterType);

/// <summary>
/// 从仓储中移除指定类型的过滤器。
/// </summary>
/// <param name="filterType">要移除的过滤器类型。</param>
/// <returns>移除了过滤器的仓储实例。</returns>
EfCoreRepository<TDbContext, TEntity> RemoveFilter(
    Type filterType);

/// <summary>
/// 插入一个新实体。
/// </summary>
/// <param name="entity">要插入的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<TEntity> InsertAsync(
    TEntity entity, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 插入一个新实体。
/// </summary>
/// <param name="entity">要插入的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
TEntity Insert(
    TEntity entity, bool autoSave = false);

/// <summary>
/// 插入多个实体。
/// </summary>
/// <param name="entities">要插入的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task InsertManyAsync(
    IEnumerable<TEntity> entities, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 插入多个实体。
/// </summary>
/// <param name="entities">要插入的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void InsertMany(
    IEnumerable<TEntity> entities, bool autoSave = false);

/// <summary>
/// 更新一个已存在的实体。
/// </summary>
/// <param name="entity">要更新的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<TEntity> UpdateAsync(
    TEntity entity, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 更新一个已存在的实体。
/// </summary>
/// <param name="entity">要更新的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
TEntity Update(
    TEntity entity, bool autoSave = false);

/// <summary>
/// 更新多个实体。
/// </summary>
/// <param name="entities">要更新的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task UpdateManyAsync(
    IEnumerable<TEntity> entities, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 更新多个实体。
/// </summary>
/// <param name="entities">要更新的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void UpdateMany(
    IEnumerable<TEntity> entities, bool autoSave = false);

/// <summary>
/// 删除一个实体。
/// </summary>
/// <param name="entity">要删除的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task DeleteAsync(
    TEntity entity, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 删除一个实体。
/// </summary>
/// <param name="entity">要删除的实体。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void Delete(
    TEntity entity, bool autoSave = false);

/// <summary>
/// 删除多个实体。
/// </summary>
/// <param name="entities">要删除的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task DeleteManyAsync(
    IEnumerable<TEntity> entities, bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 删除多个实体。
/// </summary>
/// <param name="entities">要删除的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void DeleteMany(
    IEnumerable<TEntity> entities, bool autoSave = false);

/// <summary>
/// 保存所有待处理的更改。
/// </summary>
/// <param name="cancellationToken">取消标记。</param>
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

/// <summary>
/// 保存所有待处理的更改。
/// </summary>
int SaveChanges();

/// <summary>
/// 获取所有实体。
/// </summary>
/// <param name="includeDetails">是否包含导航属性。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<List<TEntity>> GetListAsync(
    bool includeDetails = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取所有实体。
/// </summary>
/// <param name="includeDetails">是否包含导航属性。</param>
List<TEntity> GetList(
    bool includeDetails = false);

/// <summary>
/// 获取实体总数量。
/// </summary>
/// <param name="cancellationToken">取消标记。</param>
Task<long> GetCountAsync(
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取实体总数量。
/// </summary>
long GetCount();

/// <summary>
/// 获取符合条件的实体数量。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<long> GetCountAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取符合条件的实体数量。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
long GetCount(
    Expression<Func<TEntity, bool>> predicate);

/// <summary>
/// 获取分页列表。
/// </summary>
/// <param name="skipCount">跳过的记录数。</param>
/// <param name="maxResultCount">返回的最大记录数。</param>
/// <param name="sorting">排序表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<List<TEntity>> GetPagedListAsync(
    int skipCount,
    int maxResultCount,
    string? sorting = null,
    bool includeDetails = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取分页列表。
/// </summary>
/// <param name="skipCount">跳过的记录数。</param>
/// <param name="maxResultCount">返回的最大记录数。</param>
/// <param name="sorting">排序表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
List<TEntity> GetPagedList(
    int skipCount,
    int maxResultCount,
    string? sorting = null,
    bool includeDetails = false);

/// <summary>
/// 获取符合条件的实体列表。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<List<TEntity>> GetListAsync(
    Expression<Func<TEntity, bool>> predicate,
    bool includeDetails = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取符合条件的实体列表。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
List<TEntity> GetList(
    Expression<Func<TEntity, bool>> predicate,
    bool includeDetails = false);

/// <summary>
/// 获取符合条件的单个实体。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
/// <param name="cancellationToken">取消标记。</param>
Task<TEntity> GetAsync(
    Expression<Func<TEntity, bool>> predicate,
    bool includeDetails = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 获取符合条件的单个实体。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="includeDetails">是否包含导航属性。</param>
TEntity? Get(
    Expression<Func<TEntity, bool>> predicate,
    bool includeDetails = false);

/// <summary>
/// 删除符合条件的实体。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task DeleteAsync(
    Expression<Func<TEntity, bool>> predicate,
    bool autoSave = false,
    CancellationToken cancellationToken = default);

/// <summary>
/// 删除符合条件的实体。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void Delete(
    Expression<Func<TEntity, bool>> predicate,
    bool autoSave = false);

/// <summary>
/// 直接删除符合条件的实体，不应用软删除或其他过滤器。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
/// <param name="cancellationToken">取消标记。</param>
Task DeleteDirectAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

/// <summary>
/// 直接删除符合条件的实体，不应用软删除或其他过滤器。
/// </summary>
/// <param name="predicate">过滤条件表达式。</param>
void DeleteDirect(
    Expression<Func<TEntity, bool>> predicate);

/// <summary>
/// 直接删除指定实体集合，不应用任何过滤器。
/// </summary>
/// <param name="entities">要删除的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
void DeleteManyDirect(
    IEnumerable<TEntity> entities,
    bool autoSave = false);

/// <summary>
/// 直接删除指定实体集合，不应用任何过滤器。
/// </summary>
/// <param name="entities">要删除的实体集合。</param>
/// <param name="autoSave">是否自动保存更改。</param>
/// <param name="cancellationToken">取消标记。</param>
Task DeleteManyDirectAsync(
    IEnumerable<TEntity> entities,
    bool autoSave = false,
    CancellationToken cancellationToken = default);

```


## 5.WhereIf支持

EasyCore.EFCoreRepository提供一个IQueryable<T>.WhereIf的支持。

```
 IQueryable<T>.WhereIf(xxx!= null, x => x.xxx == xxx)
```
上述代码中只有 xxx!= null 时，才会执行 x => x.xxx == xxx 否则继续执行后面的代码。

## 6.数据过滤器
EasyCore.EFCoreRepository提供了两个数据过滤器，ISoftDeleteFilter和ITenantFilter分别代表软删除和租户过滤器。用户可根据自身需求编写自定义过滤器

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
删除或新增过滤器
```
    _repository
      .RemoveFilter(typeof(ITenantFilter))
      .RemoveFilter(typeof(ISoftDeleteFilter))
      .AddFilter(typeof(CustomDataFilter))
      .Delete(e => e.Name == "Test1", true);
```

# EasyCore.EFCoreUnitOfWork

EasyCore.EFCoreUnitOfWork 提供了一个SaveChangesAttribute特性，需要保存至数据库操作的方法或类加上SaveChangesAttribute特性，数据即可自动保存至数据库中。

## 1.Program注册

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

         // Use EasyCore EFCore Repository
         builder.Services.EasyCoreEFCoreRepository();
         // Use EasyCore EFCore UnitOfWork
         builder.Services.EasyCoreEFCoreUnitOfWork();

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
## 2.抽象接口

```
    public interface IUnitOfWorkTest : ITransientDependencie
    {
        /// <summary>
        /// Test the entity unit of work.
        /// </summary>
        /// <returns></returns>
        Task<TestEntity> EntityUnitOfWork();

        /// <summary>
        /// Test the transaction unit of work.
        /// </summary>
        /// <returns></returns>
        Task<TestEntity> Transaction();
    }

    public interface IUnitOfWorkTest2 : ITransientDependencie
    {
        /// <summary>
        /// Test the entity unit of work.
        /// </summary>
        /// <returns></returns>
        Task<TestEntity> EntityUnitOfWork();
    }
```

## 3.类或方法上继承 SaveChangesAttribute特性

```
/// <summary>
/// Attributes on methods.
/// </summary>
public class UnitOfWorkTest : IUnitOfWorkTest
{
    private readonly ITestEntityRepository _repository;

    public UnitOfWorkTest(ITestEntityRepository repository) => _repository = repository;

    [SaveChanges(typeof(TestDbContext))]
    public Task<TestEntity> EntityUnitOfWork() => _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });

    [SaveChanges(true, typeof(TestDbContext))]
    public Task<TestEntity> Transaction() => _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });
}

/// <summary>
/// Attributes on class.
/// </summary>
[SaveChanges(typeof(TestDbContext))]
public class UnitOfWorkTest2 : IUnitOfWorkTest2
{
    private readonly ITestEntityRepository _repository;

    public UnitOfWorkTest2(ITestEntityRepository repository) => _repository = repository;

    public Task<TestEntity> EntityUnitOfWork() => _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });
}
```

 [SaveChanges(true, typeof(TestDbContext))] 特性中，第一个参数表示是否为数据库事务，为True时执行事务保存。第二个参数为所保存数据库的DbContext对象。

# EasyCore.EFCoreEntityChange

EasyCore.EFCoreEntityChange 提供了实体变更追踪。

## 1.Program注册

```
builder.Services.AddDbContext<TestDbContext>(op =>
{
    op.UseEasyCoreEFCoreEntityChange(builder.Services); // Use EasyCore EFCore Entity Change
});

builder.Services.AddDbContext<Test2DbContext>(op =>
{
    op.UseEasyCoreEFCoreEntityChange(builder.Services); // Use EasyCore EFCore Entity Change
});

// Use EasyCore Entity Change
builder.Services.EasyCoreEFCoreEntityChange();
```

## 2.使用实体追踪

```
   public class EntityChange : IEntityUpdatedChangeHandler<TestEntity, TestEntity>, IEntityDeletedChangeHandler<TestEntity>, IEntityAddedChangeHandler<TestEntity>
   {
       private readonly ILogger<EntityChange> _logger;

       public EntityChange(ILogger<EntityChange> logger) => _logger = logger;

       public async Task OnAddedAsync(TestEntity entity)
       {
           _logger.LogInformation($"Entity added: Id:{entity.Id}; Name:{entity.Name};Age:{entity.Age};");

           await Task.CompletedTask;
       }

       public async Task OnDeletedAsync(TestEntity entity)
       {
           _logger.LogInformation($"Entity deleted: Id:{entity.Id}; Name:{entity.Name};Age:{entity.Age};");

           await Task.CompletedTask;
       }

       public Task OnUpdatedAsync(TestEntity oldEntity, TestEntity currentEntity)
       {
           _logger.LogInformation($"Entity updated: Id:{oldEntity.Id} --> Id:{currentEntity.Id}; Name:{oldEntity.Name} --> Name:{currentEntity.Name};Age:{oldEntity.Age} --> Age:{currentEntity.Age};");

           return Task.CompletedTask;
       }
   }
```
EasyCore.EFCoreEntityChange 提供了三个接口 IEntityAddedChangeHandler<TEntity>， IEntityDeletedChangeHandler<TEntity> ，IEntityUpdatedChangeHandler<TOriginalEntity, TCurrentEntity> 分别对应实体的增删改。

当实体增删改完成时，就会自动调用并执行接口方法。












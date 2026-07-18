# 🏗️ EasyCore.EFCoreRepository

[中文 README](https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/README.md) | [MongoDb 中文 README](https://gitee.com/wzhy-0521/easy-core.-efcore-repository/blob/master/README.MongoDb.md)

📚 Overview
Repository is a crucial concept in software development, widely used especially in Domain-Driven Design (DDD) 🎯 and the Data Access Layer. Repository is a design pattern 🏛️ that abstracts the data access layer, encapsulating data access logic and decoupling upper-level business logic from underlying data storage. Repository acts like an interface 📦 to the data warehouse, managing persistence operations (CRUD) for entity objects.

## 🚀 Quick Start
### 1. 📝 Program Registration

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

        // ✨ Use EasyCore EFCore Repository
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
### 2. 🏷️ Entity Inheritance
EasyCore.EFCoreRepository provides a feature-rich entity base class EasyCoreEntity, including:

🔄 Concurrency Token

🗑️ Soft Delete

🏢 Multi-Tenant ID

```
// 💡 Note: The generic type in `EasyCoreEntity<TKey>` is the primary key type of the table
public class TestEntity : EasyCoreEntity<Guid>
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```
### 3. 🔧 Repository Class Inheritance
EasyCore.EFCoreRepository provides complete repository abstraction and implementation:

Repository Interface 📜
```
public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
{
    // 💡 ITransientDependencie is the auto-injection marker from EasyCore.Dependencie
}
```
Repository Implementation ⚙️
```
public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
{
    public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) 
        : base(dbContext, serviceProvider)
    {
    }
}
```
### 4.💡 Using the Repository
#### 4.1 🎯 Basic CRUD Operations
```
[Route("api/[controller]")]
[ApiController]
public class RepositoryController : ControllerBase
{
    private readonly ITestEntityRepository _repository;

    public RepositoryController(ITestEntityRepository repository) => _repository = repository;

    // 🔍 Query
    [HttpGet]
    public async Task<TestEntity> Get()
    {
        return await _repository.GetFirstAsync(e => e.Name == "Test");
    }

    // ➕ Insert
    [HttpPost]
    public async Task Post()
    {
        await _repository.InsertAsync(new TestEntity { 
            Name = "Test", 
            Age = 10, 
            Id = Guid.NewGuid() 
        }, true);
    }

    // ✏️ Update
    [HttpPut]
    public async Task Put()
    {
        var entity = await _repository.GetFirstAsync(e => e.Name == "Test");
        entity.Age = 20;
        await _repository.UpdateAsync(entity, true);
    }

    // 🗑️ Delete (Soft Delete)
    [HttpDelete]
    public async Task Delete()
    {
        var entity = await _repository.GetFirstAsync(e => e.Age == 20);
        entity.IsDeleted = true;
        await _repository.UpdateAsync(entity, true);
    }
}
```
#### 4.2 📋 Complete API List
EasyCore.EFCoreRepository provides a rich set of API methods:

##### 🔧 Filter Management
```
EfCoreRepository<TDbContext, TEntity> AddFilter(Type filterType);

EfCoreRepository<TDbContext, TEntity> RemoveFilter(Type filterType);
```
##### ➕ Insert Operations
```
Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

TEntity Insert(TEntity entity, bool autoSave = false);

Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### ✏️ Update Operations
```
Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

TEntity Update(TEntity entity, bool autoSave = false);

Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### 🗑️ Delete Operations
```
Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default);

void Delete(TEntity entity, bool autoSave = false);

Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);

void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false);
```
##### 💾 Save Operations
```
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

int SaveChanges();
```
##### 🔍 Query Operations
```
// Get List
Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);
List<TEntity> GetList();
Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);

// Count
Task<long> GetCountAsync(CancellationToken cancellationToken = default);
long GetCount();
Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
long GetCount(Expression<Func<TEntity, bool>> predicate);

// Paged query (optional ordering)
Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
List<TEntity> GetPagedList(int skipCount, int maxResultCount);
Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, Expression<Func<TEntity, object>> orderBy, bool ascending = true, CancellationToken cancellationToken = default);
List<TEntity> GetPagedList(int skipCount, int maxResultCount, Expression<Func<TEntity, object>> orderBy, bool ascending = true);

// Single entity (GetFirstAsync throws if not found; GetFirst / GetFirstOrDefaultAsync return null)
Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
TEntity? GetFirst(Expression<Func<TEntity, bool>> predicate);
```
##### ⚡ Direct Delete Operations
```
// DeleteDirect* bypasses soft-delete/tenant filters
Task DeleteDirectAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

void DeleteDirect(Expression<Func<TEntity, bool>> predicate);

void DeleteManyDirect(IEnumerable<TEntity> entities, bool autoSave = false);

Task DeleteManyDirectAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default);
```
### 5.🎛️ Advanced Features
#### 🎯 WhereIf Support
EasyCore.EFCoreRepository provides intelligent conditional query support:

```
IQueryable<T>.WhereIf(xxx != null, x => x.xxx == xxx)
```
✨ Feature: The subsequent filter condition is only executed when the xxx != null condition is met; otherwise, the code continues execution.

### 6. 🔍 Data Filters
EasyCore.EFCoreRepository includes two practical built-in data filters:

🗑️ ISoftDeleteFilter - Soft Delete Filter

🏢 ITenantFilter - Tenant Filter

#### Custom Filter Example 🎨:
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
#### Dynamic Filter Management ⚡:
```
_repository
    .RemoveFilter(typeof(ITenantFilter))      // 🗑️ Remove Tenant Filter
    .RemoveFilter(typeof(ISoftDeleteFilter))  // 🗑️ Remove Soft Delete Filter  
    .AddFilter(typeof(CustomDataFilter))      // ➕ Add Custom Filter
    .Delete(e => e.Name == "Test1", true);    // 🎯 Execute Operation
```
# 🔄 EasyCore.UnitOfWork
## 🎯 Unit of Work Pattern
EasyCore.UnitOfWork provides the SaveChangesAttribute feature, making data persistence simple and efficient! ✨

### 1. 📝 Program Registration
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

        // ✨ Use EasyCore EFCore Repository
        builder.Services.EasyCoreEFCoreRepository();
        // 🔄 Use EasyCore UnitOfWork
        builder.Services.EasyCoreUnitOfWork()
            .RegisterSaveChangesFor<IUnitOfWorkTest, UnitOfWorkTest>();

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
### 2. 📜 Abstract Interface Definition
```
public interface IUnitOfWorkTest : ITransientDependencie
{
    /// <summary>
    /// 🎯 Test Entity Unit of Work
    /// </summary>
    Task<TestEntity> EntityUnitOfWork();

    /// <summary>
    /// 💰 Test Transaction Unit of Work  
    /// </summary>
    Task<TestEntity> Transaction();
}

public interface IUnitOfWorkTest2 : ITransientDependencie
{
    /// <summary>
    /// 🎯 Test Entity Unit of Work
    /// </summary>
    Task<TestEntity> EntityUnitOfWork();
}
```
### 3. 🏷️ Using the SaveChangesAttribute
#### Method Level Usage 🎯:
```
public class UnitOfWorkTest : IUnitOfWorkTest
{
    private readonly ITestEntityRepository _repository;

    public UnitOfWorkTest(ITestEntityRepository repository) => _repository = repository;

    [SaveChanges(typeof(TestDbContext))]
    public Task<TestEntity> EntityUnitOfWork() 
        => _repository.InsertAsync(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });

    [SaveChanges(true, typeof(TestDbContext))]  // 💰 Enable Transaction
    public Task<TestEntity> Transaction() 
        => _repository.InsertAsync(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() });
}
```
#### Class Level Usage 🏛️:
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
#### 💡 Attribute Parameter Description:

First parameter: Whether it is a database transaction; when true, performs a transactional save 💰

Second parameter: Specifies the database DbContext object to save

# 🔍 EasyCore.EntityChange
## 📊 Entity Change Tracking
EasyCore.EFCoreEntityChange provides powerful entity change tracking capabilities! 🕵️

### 1. 📝 Program Registration
```
builder.Services.EasyCoreEntityChange()
    .AddHandler<EntityChange>();

builder.Services.AddDbContext<TestDbContext>((sp, op) =>
{
    op.UseEasyCoreEntityChange(sp);
});
```
### 2. 🎯 Using Entity Change Tracking
```
public class EntityChange : 
    IEntityUpdatedChangeHandler<TestEntity>, 
    IEntityDeletedChangeHandler<TestEntity>, 
    IEntityAddedChangeHandler<TestEntity>
{
    private readonly ILogger<EntityChange> _logger;

    public EntityChange(ILogger<EntityChange> logger) => _logger = logger;

    // ➕ Entity Added Handler
    public async Task OnAddedAsync(TestEntity entity)
    {
        _logger.LogInformation($"🆕 Entity Added: Id:{entity.Id}; Name:{entity.Name}; Age:{entity.Age};");
        await Task.CompletedTask;
    }

    // 🗑️ Entity Deleted Handler  
    public async Task OnDeletedAsync(TestEntity entity)
    {
        _logger.LogInformation($"🗑️ Entity Deleted: Id:{entity.Id}; Name:{entity.Name}; Age:{entity.Age};");
        await Task.CompletedTask;
    }

    // ✏️ Entity Updated Handler
    public Task OnUpdatedAsync(TestEntity oldEntity, TestEntity currentEntity)
    {
        _logger.LogInformation($"✏️ Entity Updated: " +
            $"Id:{oldEntity.Id} → {currentEntity.Id}; " +
            $"Name:{oldEntity.Name} → {currentEntity.Name}; " +
            $"Age:{oldEntity.Age} → {currentEntity.Age};");
        return Task.CompletedTask;
    }
}
```
#### 🎯 Supported Change Interfaces:
```
IEntityAddedChangeHandler<TEntity> - Entity Added Handler ➕

IEntityDeletedChangeHandler<TEntity> - Entity Deleted Handler 🗑️

IEntityUpdatedChangeHandler<TEntity> - Entity Updated Handler ✏️
```
# 💎 Custom Entity And Custom Data Filter

## 1.  User-Defined Database Entity🦄

Users can configure custom user entities based on their specific project requirements.

```
    public class CustomEntity : EasyCoreEntity<Guid>
    {
        public string CreateId{ get; set; }
    }

    public class TestCustomEntity : CustomEntity
    {

    }
```

The CustomEntity represents a user-defined entity object. This custom entity contains a CreateId field. During the save operation, the IEntityAddedChangeHandler<TEntity> interface can be utilized to automatically save the current user's ID.


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
## 2.  User-defined Data Filters🎁

Users can configure custom data filters based on their specific project requirements.


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
Override the OnApplyPersistingFilters method to add or modify persistent filter settings.

#### ✨ Feature: When entity add, delete, or update operations are completed, the system automatically calls the corresponding interface methods, enabling seamless change tracking!

# 🏷️CustomRepository
A user-defined repository. Since the entity fields vary across different systems, EasyCore does not restrict the entity fields or field types used by users. For example, fields like "creator ID" can be either Guid or long.

CustomEntityRepository provides three overridable methods: OnBeforeAdd (before adding), OnBeforeUpdate (before updating), and OnBeforeDelete (before deleting).

This allows us to automatically set field values during create, update, and delete operations without manual intervention. CustomRepository handles this automatically.

```
  public class CustomEntityRepository<TDbContext, TEntity> : EfCoreRepository<TDbContext, TEntity>
      where TDbContext : DbContext
      where TEntity : class, IEntity
  {
      public CustomEntityRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
      {

      }

      /// <summary>
      /// Custom method to set the CreateId property of the entity before adding it to the database.
      /// </summary>
      /// <param name="entity"></param>
      public override void OnBeforeAdd(TEntity entity)
      {
          if (entity is CustomEntity customEntity)
          {
              customEntity.CreateId = "Test";
          }
      }

      /// <summary>
      /// Custom method to set the UpdateId property of the entity before updating it in the database.
      /// </summary>
      /// <param name="entity"></param>
      public override void OnBeforeUpdate(TEntity entity)
      {
          // Do something entity before update
      }

      /// <summary>
      /// Custom method to set the DeleteId property of the entity before deleting it from the database.
      /// </summary>
      /// <param name="entity"></param>
      public override void OnBeforeDelete(TEntity entity)
      {
          // Do something entity before delete
      }
```

```
  public class TestCustomEntityRepository : CustomEntityRepository<TestDbContext, TestCustomEntity>, ITestCustomEntityRepository
  {
      public TestCustomEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
      {

      }
  }
```
Full control is given to the user.

# 🎉 Summary
The EasyCore.EFCoreRepository series of components provides:

🏗️ Complete Repository Pattern Implementation

🔄 Smart Unit of Work Management

🔍 Powerful Entity Change Tracking

🎯 Rich Querying and Filtering Capabilities

⚡ High-Performance Data Access

Making your data access layer more elegant, powerful, and maintainable! ✨
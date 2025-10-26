# EasyCore.EFCoreRepository


Repository 是软件开发中的一个重要概念，尤其在 领域驱动设计（DDD） 和 数据访问层 中广泛使用。Repository 是一种 抽象数据访问层的设计模式，它封装了数据访问逻辑，使上层业务逻辑与底层数据存储解耦。Repository 就像是数据仓库的接口，用于管理实体对象的持久化（增删改查）操作。

 **使用说明：** 

1.Program注册

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

2.实体继承

EasyCore.EFCoreRepository 提供了一个实体基类 EasyCoreEntity，包含并发标记/软删除/以及多租户ID。

```
    // 注：EasyCoreEntity<TKey>中的泛型类型输入为表的主键。
    public class TestEntity : EasyCoreEntity<Guid>
    {
        public string Name { get; set; }

        public int Age { get; set; }

    }
```

3.仓储类继承

EasyCore.EFCoreRepository 提供了一个仓储基类 EfCoreRepository<TDbContext, TEntity> 和仓储抽象 IRepository<TEntity>。


```
    public interface ITestEntityRepository : IRepository<TestEntity>, ITransientDependencie
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

4.使用仓储


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

5.WhereIf支持

EasyCore.EFCoreRepository提供一个IQueryable<T>.WhereIf的支持。


```
 IQueryable<T>.WhereIf(xxx!= null, x => x.xxx == xxx)
```
上述代码中只有 xxx!= null 时，才会执行 x => x.xxx == xxx 否则继续执行后面的代码。

EasyCore.EFCoreRepository就可以自动实现实体的CURD操作。

### EasyCore.EFCoreUnitOfWork

EasyCore.EFCoreUnitOfWork 提供了一个SaveChangesAttribute特性，需要保存至数据库操作的方法或类加上SaveChangesAttribute特性，数据即可自动保存至数据库中。

1.Program注册

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
2.抽象接口

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

3.类或方法上继承 SaveChangesAttribute特性

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









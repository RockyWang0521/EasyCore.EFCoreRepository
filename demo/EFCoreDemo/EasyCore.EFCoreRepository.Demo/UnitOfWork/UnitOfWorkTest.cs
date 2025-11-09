using EasyCore.UnitOfWork;
using EFCore.Entity;
using EFCore.Repository;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EasyCore.EFCoreRepository.Demo.UnitOfWork
{
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
}

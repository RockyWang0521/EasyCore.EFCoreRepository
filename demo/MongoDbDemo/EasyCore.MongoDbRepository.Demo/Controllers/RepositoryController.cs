using EasyCore.MongoDbRepository.DataFilter;
using Microsoft.AspNetCore.Mvc;
using MongoDb.Entity;
using MongoDb.Repository;

namespace EasyCore.MongoDbRepository.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryController : ControllerBase
    {
        private readonly ITestEntityRepository _repository;

        public RepositoryController(ITestEntityRepository repository) => _repository = repository;

        [HttpGet]
        public async Task<ActionResult<TestEntity>> Get()
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Name == "Test");
            if (entity == null) return NotFound();
            return entity;
        }

        /// <summary>
        /// Includes soft-deleted documents (removes <see cref="ISoftDeleteFilter"/>).
        /// </summary>
        [HttpGet("with-deleted")]
        public async Task<List<TestEntity>> GetWithDeleted()
        {
            return await _repository
                .RemoveFilter(typeof(ISoftDeleteFilter))
                .GetListAsync(e => e.Name == "Test");
        }

        /// <summary>
        /// Cross-tenant query (removes <see cref="ITenantFilter"/>).
        /// </summary>
        [HttpGet("all-tenants")]
        public async Task<List<TestEntity>> GetAllTenants()
        {
            return await _repository
                .RemoveFilter(typeof(ITenantFilter))
                .GetListAsync();
        }

        /// <summary>
        /// No soft-delete / tenant filters — returns all matching documents.
        /// </summary>
        [HttpGet("unfiltered")]
        public async Task<List<TestEntity>> GetUnfiltered()
        {
            return await _repository
                .RemoveFilter(typeof(ISoftDeleteFilter))
                .RemoveFilter(typeof(ITenantFilter))
                .GetListAsync(e => e.Name == "Test");
        }

        [HttpPost]
        public void Post()
        {
            // Sync SaveChanges also dispatches EntityChange OnAddedAsync / OnUpdatedAsync / OnDeletedAsync.
            _repository.Insert(new TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() }, autoSave: true);
        }

        [HttpPut]
        public async Task Put()
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Name == "Test");
            if (entity == null) return;

            entity.Age = 20;
            _repository.Update(entity, autoSave: true);
        }

        [HttpDelete]
        public void Delete()
        {
            _repository.Delete(e => e.Name == "Test", autoSave: true);
        }
    }
}

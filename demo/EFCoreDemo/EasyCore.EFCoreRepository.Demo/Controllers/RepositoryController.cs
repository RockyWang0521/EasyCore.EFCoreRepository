using EasyCore.EFCoreRepository.DataFilter;
using EFCore.Entity;
using EFCore.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.EFCoreRepository.Demo.Controllers
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
        /// Includes soft-deleted rows (removes <see cref="ISoftDeleteFilter"/>).
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
        /// No soft-delete / tenant filters — returns all matching rows.
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
        public async Task<TestEntity> Post()
        {
            return await _repository.InsertAsync(new TestEntity
            {
                Name = "Test",
                Age = 10,
                Id = Guid.NewGuid()
            }, true);
        }

        [HttpPut]
        public async Task Put()
        {
            var entity = await _repository.GetFirstOrDefaultAsync(e => e.Name == "Test" && e.Age == 10);
            if (entity == null) return;

            entity.Age = 20;
            await _repository.UpdateAsync(entity, true);
        }

        [HttpDelete]
        public async Task Delete()
        {
            await _repository.DeleteAsync(e => e.Name == "Test", autoSave: true);
        }
    }
}

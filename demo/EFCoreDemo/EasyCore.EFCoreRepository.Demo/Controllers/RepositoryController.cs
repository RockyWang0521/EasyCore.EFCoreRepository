using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.Demo.DataFilter;
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
        public async Task<TestEntity> Get()
        {
            return await _repository.GetFirstAsync(e => e.Name == "Test");
        }

        [HttpPost]
        public async Task Post()
        {
            await _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() }, true);
        }

        [HttpPut]
        public async Task Put()
        {
            var entity = await _repository.GetFirstAsync(e => e.Name == "Test" && e.Age == 10);

            if (entity == null) return;

            entity.Age = 20;

            await _repository.UpdateAsync(entity, true);
        }

        [HttpDelete]
        public async Task Delete()
        {
            _repository
                .RemoveFilter(typeof(ITenantFilter))
                .RemoveFilter(typeof(ISoftDeleteFilter))
                .AddFilter(typeof(CustomDataFilter))
                .AsQueryable().Where(e => e.Name == "Test").ToList();

            await Task.CompletedTask;
        }
    }
}

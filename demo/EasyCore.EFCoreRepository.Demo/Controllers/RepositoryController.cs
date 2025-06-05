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
            return await _repository.GetAsync(e => e.Name == "Test");
        }

        [HttpPost]
        public async void Post()
        {
            await _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Guid = Guid.NewGuid() }, true);
        }
    }
}

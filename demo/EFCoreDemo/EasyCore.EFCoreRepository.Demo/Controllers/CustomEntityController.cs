using EFCore.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.EFCoreRepository.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomEntityController : ControllerBase
    {
        private readonly ITestCustomEntityRepository _repository;

        public CustomEntityController(ITestCustomEntityRepository repository) => _repository = repository;

        [HttpPost]
        public async Task Post()
        {
            await _repository.InsertAsync(new EFCore.Entity.TestCustomEntity(), true);
        }
    }
}

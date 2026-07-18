using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.Demo.DataFilter;
using EFCore.Entity;
using EFCore.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            await _repository.InsertAsync(new TestCustomEntity
            {
                Id = Guid.NewGuid(),
                CreateId = "Test"
            }, true);
        }

        /// <summary>
        /// Can freely add, remove, or modify data filters
        /// </summary>
        [HttpPost("filter")]
        public async Task<List<TestCustomEntity>> Filter()
        {
            return await _repository
                .RemoveFilter(typeof(ITenantFilter))
                .RemoveFilter(typeof(ISoftDeleteFilter))
                .AddFilter(typeof(CustomDataFilter))
                .AsQueryable()
                .Where(e => e.CreateId == "Test")
                .ToListAsync();
        }
    }
}

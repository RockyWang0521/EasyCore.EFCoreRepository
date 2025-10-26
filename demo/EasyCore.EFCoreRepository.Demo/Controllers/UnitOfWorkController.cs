using EasyCore.EFCoreRepository.Demo.UnitOfWork;
using EFCore.Entity;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.EFCoreRepository.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitOfWorkController : ControllerBase
    {
        private readonly IUnitOfWorkTest _unitOfWork;
        private readonly IUnitOfWorkTest2 _unitOfWork2;

        public UnitOfWorkController(IUnitOfWorkTest unitOfWork, IUnitOfWorkTest2 unitOfWork2)
        {
            _unitOfWork = unitOfWork;
            _unitOfWork2 = unitOfWork2;
        }

        [HttpPost("/UnitOfWork")]
        public async Task<TestEntity> UnitOfWork() => await _unitOfWork.EntityUnitOfWork();

        [HttpPost("/transaction")]
        public async Task<TestEntity> Transaction() => await _unitOfWork.Transaction();

        [HttpPost("/UnitOfWork2")]
        public async Task<TestEntity> UnitOfWork2() => await _unitOfWork2.EntityUnitOfWork();
    }
}

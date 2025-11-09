using EasyCore.Dependencie;
using EFCore.Entity;

namespace EasyCore.EFCoreRepository.Demo.UnitOfWork
{
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
}

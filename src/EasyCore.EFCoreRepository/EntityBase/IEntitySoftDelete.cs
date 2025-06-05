namespace EasyCore.EFCoreRepository.EntityBase
{
    public interface IEntitySoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}

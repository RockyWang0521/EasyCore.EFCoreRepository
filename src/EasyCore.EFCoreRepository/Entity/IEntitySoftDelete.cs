namespace EasyCore.EFCoreRepository.Entity
{
    public interface IEntitySoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}

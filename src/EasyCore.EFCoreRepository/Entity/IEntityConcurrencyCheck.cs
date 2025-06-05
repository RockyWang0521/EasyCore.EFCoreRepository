namespace EasyCore.EFCoreRepository.Entity
{
    public interface IEntityConcurrencyCheck
    {
        public string ConcurrencyStamp { get; set; }
    }
}

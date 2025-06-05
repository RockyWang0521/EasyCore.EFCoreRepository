namespace EasyCore.EFCoreRepository.EntityBase
{
    public interface IEntityConcurrencyCheck
    {
        public string ConcurrencyStamp { get; set; }
    }
}

namespace EasyCore.EFCoreRepository.EntityBase
{
    /// <summary>
    /// Interface for entity with concurrency check.
    /// </summary>
    public interface IEntityConcurrencyCheck
    {
        public string ConcurrencyStamp { get; set; }
    }
}

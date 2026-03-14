namespace EasyCore.EntityChange
{
    /// <summary>
    /// Options for entity change handler dispatch.
    /// </summary>
    public sealed class EntityChangeOptions
    {
        /// <summary>
        /// When true, handler exceptions are logged and swallowed.
        /// Default is false (exceptions propagate and abort SaveChanges).
        /// </summary>
        public bool SuppressHandlerExceptions { get; set; }
    }
}

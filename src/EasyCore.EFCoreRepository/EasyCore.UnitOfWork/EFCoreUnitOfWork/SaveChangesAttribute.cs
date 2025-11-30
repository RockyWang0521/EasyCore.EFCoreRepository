using Microsoft.EntityFrameworkCore;

namespace EasyCore.UnitOfWork
{
    /// <summary>
    /// Unit of Work SaveChanges Feature Tag
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class SaveChangesAttribute : Attribute
    {
        public bool IsTransaction { get; set; } = false;

        public Type DbContextType { get; }

        public SaveChangesAttribute(Type dbContextType)
        {
            if (!typeof(DbContext).IsAssignableFrom(dbContextType))
                throw new ArgumentException("Provided type must be a DbContext.", nameof(dbContextType));

            DbContextType = dbContextType;
        }

        public SaveChangesAttribute(bool isTransaction, Type dbContextType)
        {
            if (!typeof(DbContext).IsAssignableFrom(dbContextType))
                throw new ArgumentException("Provided type must be a DbContext.", nameof(dbContextType));

            DbContextType = dbContextType;

            IsTransaction = isTransaction;
        }
    }
}

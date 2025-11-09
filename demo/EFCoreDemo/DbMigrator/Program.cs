using EFCoreDbContext.EntityFrameworkCore.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace DbMigrator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var dbContext = new TestDbContext();

            dbContext.Database.Migrate();
        }
    }
}

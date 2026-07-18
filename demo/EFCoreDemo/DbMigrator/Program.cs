using EFCoreDbContext.EntityFrameworkCore.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace DbMigrator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var dbContext = new TestDbContext();

            // Demo helper: reset schema when local DB is out of sync with migrations.
            // Usage: DbMigrator.exe --recreate
            if (args.Any(a => string.Equals(a, "--recreate", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Recreating database...");
                dbContext.Database.EnsureDeleted();
            }

            try
            {
                dbContext.Database.Migrate();
                Console.WriteLine("Migrate completed.");
            }
            catch (Exception ex) when (ex.GetBaseException().Message.Contains("already an object named", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine(
                    "Database schema exists but __EFMigrationsHistory is missing or out of sync.\n" +
                    "Run with --recreate to drop and recreate the Demo database:\n" +
                    "  dotnet run --project demo/EFCoreDemo/DbMigrator -- --recreate");
                throw;
            }
        }
    }
}

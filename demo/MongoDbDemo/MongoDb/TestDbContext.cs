using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDb.Entity;
using MongoDB.EntityFrameworkCore.Extensions;

namespace MongoDbContext.EntityFrameworkCore.EFDbContext
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() { }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public virtual DbSet<TestEntity> TestEntity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Keep DI-registered interceptors (EntityChange); only set provider when not configured.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMongoDB(
                    "mongodb://admin:123456@localhost:27017/testdb?authSource=admin",
                    "testmongodb");
            }

            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(option =>
            {
                option.AddConsole();
            }));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.ToCollection("TestEntity");
            });
        }
    }
}

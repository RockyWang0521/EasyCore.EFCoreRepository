using EFCore.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDbContext.EntityFrameworkCore.EFDbContext
{
    public class TestDbContext : DbContext
    {
        public TestDbContext() { }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public virtual DbSet<TestEntity> TestEntity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer("Server=192.168.157.142;Database=Demo;User Id=sa;Password=Sa123456;TrustServerCertificate=True;Connect Timeout=10;")
                .UseLoggerFactory(LoggerFactory.Create(option =>
                {
                    option.AddConsole();
                }));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Chinese_PRC_CI_AS");

            modelBuilder.Entity<TestEntity>(e =>
            {
                e.ToTable("TestEntity");
            });
        }
    }
}

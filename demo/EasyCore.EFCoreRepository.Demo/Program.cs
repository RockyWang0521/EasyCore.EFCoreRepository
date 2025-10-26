using EasyCore.Dependencie;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;
using EasyCore.EFCoreUnitOfWork;

namespace EasyCore.EFCoreRepository.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.EasyCoreDependencie();
            builder.Services.AddDbContext<TestDbContext>();

            // Use EasyCore EFCore Repository
            builder.Services.EasyCoreEFCoreRepository();

            // Use EasyCore EFCore UnitOfWork
            builder.Services.EasyCoreEFCoreUnitOfWork();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

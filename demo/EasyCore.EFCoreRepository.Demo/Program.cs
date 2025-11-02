using EasyCore.Dependencie;
using EasyCore.EFCoreEntityChange;
using EasyCore.EFCoreUnitOfWork;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

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
            builder.Services.AddDbContext<TestDbContext>(op =>
            {
                op.UseEasyCoreEFCoreEntityChange(builder.Services); // Use EasyCore EFCore Entity Change
            });

            // Use EasyCore EFCore Repository
            builder.Services.EasyCoreEFCoreRepository();

            // Use EasyCore EFCore UnitOfWork
            builder.Services.EasyCoreEFCoreUnitOfWork();

            // Use EasyCore Entity Change
            builder.Services.EasyCoreEFCoreEntityChange();

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

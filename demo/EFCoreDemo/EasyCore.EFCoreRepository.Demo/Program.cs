using EasyCore.Dependencie;
using EasyCore.EntityChange;
using EasyCore.EFCoreRepository;
using EasyCore.EFCoreRepository.Demo.UnitOfWork;
using EasyCore.UnitOfWork;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddEasyCoreEntityChange();

            builder.Services.AddDbContext<TestDbContext>();

            builder.Services.AddEasyCoreEFCoreRepository();

            builder.Services.AddEasyCoreUnitOfWork();

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

using EasyCore.Dependencie;
using EasyCore.EntityChange;
using EasyCore.EFCoreRepository;
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

            // 1) 注册 EntityChange（默认扫描全部 Handler）
            builder.Services.AddEasyCoreEntityChange();

            // 2) 在每个 AddDbContext 里显式挂拦截器（多个 DbContext 就写多次）
            builder.Services.AddDbContext<TestDbContext>((sp, options) =>
            {
                options.UseEasyCoreEntityChange(sp);
            });
            // builder.Services.AddDbContext<OtherDbContext>((sp, options) =>
            // {
            //     options.UseSqlServer("...");
            //     options.UseEasyCoreEntityChange(sp); // 需要变更追踪的才挂
            // });

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

using EasyCore.Dependencie;
using EasyCore.EntityChange;
using EasyCore.EFCoreRepository;
using EasyCore.EFCoreRepository.Demo.UnitOfWork;
using EasyCore.UnitOfWork;
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

            // Use EasyCore Entity Change (register before AddDbContext so interceptor can resolve)
            builder.Services.EasyCoreEntityChange()
                .AddHandler<EasyCore.EFCoreRepository.Demo.EntityChange.EntityChange>();

            builder.Services.AddDbContext<TestDbContext>((sp, op) =>
            {
                op.UseEasyCoreEntityChange(sp);
            });

            // Use EasyCore EFCore Repository
            builder.Services.EasyCoreEFCoreRepository();

            // Use EasyCore EFCore UnitOfWork (explicit registration)
            builder.Services.EasyCoreUnitOfWork()
                .RegisterSaveChangesFor<IUnitOfWorkTest, UnitOfWorkTest>()
                .RegisterSaveChangesFor<IUnitOfWorkTest2, UnitOfWorkTest2>();

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

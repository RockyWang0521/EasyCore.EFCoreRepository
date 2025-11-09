using EasyCore.Dependencie;
using EasyCore.EntityChange;
using MongoDbContext.EntityFrameworkCore.EFDbContext;

namespace EasyCore.MongoDbRepository.Demo
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
                op.UseEasyCoreEntityChange(builder.Services); // Use EasyCore EFCore Entity Change
            });

            // Use EasyCore MongoDb Repository
            builder.Services.EasyCoreMongoDbRepository();

            // Use EasyCore Entity Change
            builder.Services.EasyCoreEntityChange();

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

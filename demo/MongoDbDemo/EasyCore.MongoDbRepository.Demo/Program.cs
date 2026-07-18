using EasyCore.Dependencie;
using EasyCore.EntityChange;
using EasyCore.MongoDbRepository;
using Microsoft.EntityFrameworkCore;
using MongoDbContext.EntityFrameworkCore.EFDbContext;
using MongoDB.EntityFrameworkCore.Extensions;

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

            builder.Services.EasyCoreEntityChange()
                .AddHandler<EasyCore.MongoDbRepository.Demo.EntityChange.EntityChange>();

            builder.Services.AddDbContext<TestDbContext>((sp, op) =>
            {
                op.UseMongoDB(
                    "mongodb://admin:123456@localhost:27017/testdb?authSource=admin",
                    "testmongodb");
                op.UseEasyCoreEntityChange(sp);
            });

            builder.Services.EasyCoreMongoDbRepository();

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

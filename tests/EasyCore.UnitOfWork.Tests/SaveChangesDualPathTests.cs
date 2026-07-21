using System.Reflection;
using EasyCore.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.UnitOfWork.Tests;

public interface IIfaceUowService
{
    [SaveChanges(typeof(UowDbContext))]
    Task InsertAsync();
}

public class IfaceUowService : IIfaceUowService
{
    private readonly UowDbContext _db;

    public IfaceUowService(UowDbContext db) => _db = db;

    public Task InsertAsync()
    {
        _db.Entities.Add(new UowEntity { Id = Guid.NewGuid(), Name = "iface" });
        return Task.CompletedTask;
    }
}

public class SaveChangesAttributeLocatorTests
{
    [Fact]
    public void Find_Resolves_Attribute_On_Interface_Method()
    {
        var method = typeof(IfaceUowService).GetMethod(nameof(IfaceUowService.InsertAsync))!;
        var attr = SaveChangesAttributeLocator.Find(typeof(IfaceUowService), method);
        Assert.NotNull(attr);
        Assert.Equal(typeof(UowDbContext), attr!.DbContextType);
    }

    [Fact]
    public async Task Interceptor_Honors_Interface_Method_Attribute()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UowDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddEasyCoreUnitOfWork(enableAssemblyScanning: false)
            .RegisterSaveChangesFor<IIfaceUowService, IfaceUowService>();

        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IIfaceUowService>();
        var db = scope.ServiceProvider.GetRequiredService<UowDbContext>();

        await svc.InsertAsync();

        Assert.True(db.SaveChangesAsyncCallCount >= 1);
        Assert.Single(await db.Entities.ToListAsync());
    }
}

public class SaveChangesInterfaceAttributeConventionTests
{
    [SaveChanges(typeof(UowDbContext))]
    public interface IUowApiContract
    {
        Task PingAsync();
    }

    public sealed class UowApiController : ControllerBase, IUowApiContract
    {
        public Task PingAsync() => Task.CompletedTask;
    }

    [Fact]
    public void Convention_Copies_Interface_Attribute_Onto_Action()
    {
        var controllerType = typeof(UowApiController).GetTypeInfo();
        var actionMethod = typeof(UowApiController).GetMethod(nameof(UowApiController.PingAsync))!;
        var controller = new ControllerModel(controllerType, Array.Empty<object>());
        var action = new ActionModel(actionMethod, Array.Empty<object>()) { Controller = controller };
        controller.Actions.Add(action);

        var app = new ApplicationModel();
        app.Controllers.Add(controller);

        new SaveChangesInterfaceAttributeConvention().Apply(app);

        Assert.Contains(action.Filters.OfType<SaveChangesAttribute>(), a => a.DbContextType == typeof(UowDbContext));
        Assert.Contains(action.Filters, f => f is IFilterFactory);
    }
}

public class SaveChangesDiRegistrationTests
{
    [Fact]
    public void AddEasyCoreUnitOfWork_Registers_MvcOptions_Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEasyCoreUnitOfWork(enableAssemblyScanning: false);

        Assert.Contains(
            services,
            d => d.ServiceType.Name.Contains("IConfigureOptions", StringComparison.Ordinal)
                 && d.ImplementationType == typeof(SaveChangesMvcOptionsSetup));
    }
}

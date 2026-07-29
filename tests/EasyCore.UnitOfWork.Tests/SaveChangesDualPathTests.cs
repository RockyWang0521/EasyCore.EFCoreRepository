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

    [SaveChanges(typeof(UowDbContext))]
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
    public async Task Proxy_Honors_Implementation_Method_Attribute()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UowDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        // Register services first, then UnitOfWork ApplyProxies.
        services.AddTransient<IIfaceUowService, IfaceUowService>();
        services.AddEasyCoreUnitOfWork();

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
        services.AddEasyCoreUnitOfWork();

        Assert.Contains(
            services,
            d => d.ServiceType.Name.Contains("IConfigureOptions", StringComparison.Ordinal)
                 && d.ImplementationType == typeof(SaveChangesMvcOptionsSetup));
    }
}

public class JobStyleUnitOfWorkProxyTests
{
    /// <summary>
    /// Simulates EasyCore.Hangfire JobWrapper&lt;T&gt;: concrete T + IEasyCoreHangfireJob registered;
    /// wrapper resolves T. [SaveChanges] on the job must still run.
    /// </summary>
    [Fact]
    public async Task Concrete_hangfire_job_style_type_is_proxied_and_saves_changes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UowDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddTransient<SampleHangfireUowJob>();
        services.AddTransient<IEasyCoreHangfireJob>(sp => sp.GetRequiredService<SampleHangfireUowJob>());
        services.AddEasyCoreUnitOfWork(enableAssemblyScanning: false);

        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<SampleHangfireUowJob>();
        var db = scope.ServiceProvider.GetRequiredService<UowDbContext>();

        await job.ExecuteAsync();

        Assert.True(db.SaveChangesAsyncCallCount >= 1);
        Assert.Single(await db.Entities.ToListAsync());
    }
}

// Name matters: JobStyleTypeRules treats IEasyCoreHangfireJob as a framework job interface.
public interface IEasyCoreHangfireJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

public class SampleHangfireUowJob : IEasyCoreHangfireJob
{
    private readonly UowDbContext _db;

    public SampleHangfireUowJob(UowDbContext db) => _db = db;

    [SaveChanges(typeof(UowDbContext))]
    public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _db.Entities.Add(new UowEntity { Id = Guid.NewGuid(), Name = "hangfire-job" });
        return Task.CompletedTask;
    }
}

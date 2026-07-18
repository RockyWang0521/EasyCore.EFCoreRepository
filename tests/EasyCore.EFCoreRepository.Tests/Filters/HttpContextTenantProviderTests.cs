using EasyCore.EFCoreRepository.DataFilter;
using Microsoft.AspNetCore.Http;

namespace EasyCore.EFCoreRepository.Tests.Filters;

public class HttpContextTenantProviderTests
{
    [Fact]
    public void Prefers_Items_Over_Header()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Items["TenantId"] = "from-items";
        accessor.HttpContext.Request.Headers["X-Tenant-Id"] = "from-header";

        var provider = new HttpContextTenantProvider(accessor);
        Assert.Equal("from-items", provider.GetTenantId());
    }

    [Fact]
    public void Falls_Back_To_X_Tenant_Id_Header()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext.Request.Headers["X-Tenant-Id"] = "tenant-header";

        var provider = new HttpContextTenantProvider(accessor);
        Assert.Equal("tenant-header", provider.GetTenantId());
    }

    [Fact]
    public void Returns_Null_When_No_Tenant()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var provider = new HttpContextTenantProvider(accessor);
        Assert.Null(provider.GetTenantId());
    }
}

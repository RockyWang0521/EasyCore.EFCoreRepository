using Microsoft.AspNetCore.Http;

namespace EasyCore.EFCoreRepository.DataFilter
{
    /// <summary>
    /// Resolves tenant id aligned with AspNetCore.Mvc <c>ICurrentTenant</c>:
    /// <c>HttpContext.Items["TenantId"]</c> first, then <c>X-Tenant-Id</c> header.
    /// </summary>
    public sealed class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return null;

            if (httpContext.Items.TryGetValue("TenantId", out var item) && item is not null)
            {
                var fromItems = item.ToString();
                if (!string.IsNullOrWhiteSpace(fromItems))
                    return fromItems;
            }

            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var header))
            {
                var value = header.ToString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }
    }
}

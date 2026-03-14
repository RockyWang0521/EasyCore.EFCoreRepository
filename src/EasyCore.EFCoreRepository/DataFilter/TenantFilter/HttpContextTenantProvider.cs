using Microsoft.AspNetCore.Http;

namespace EasyCore.EFCoreRepository.DataFilter
{
    /// <summary>
    /// Resolves tenant id from <see cref="HttpContext.Items"/> key "TenantId".
    /// </summary>
    public sealed class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetTenantId() =>
            _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Registers the MVC convention that merges interface <see cref="SaveChangesAttribute"/> onto actions.
/// </summary>
internal sealed class SaveChangesMvcOptionsSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Conventions.Add(new SaveChangesInterfaceAttributeConvention());
    }
}

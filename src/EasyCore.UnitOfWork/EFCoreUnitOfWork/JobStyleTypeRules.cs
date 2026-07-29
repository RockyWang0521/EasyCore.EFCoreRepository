namespace EasyCore.UnitOfWork;

/// <summary>
/// Shared rules for Quartz / Hangfire job types that are resolved as concrete <c>T</c>
/// by <c>JobWrapper&lt;T&gt;</c> (not via <c>IJob</c> / <c>IEasyCoreJob</c> / <c>IEasyCoreHangfireJob</c>).
/// </summary>
/// <remarks>
/// Interface names are matched by simple name so this package does not need a reference
/// to Quartz or Hangfire assemblies.
/// </remarks>
internal static class JobStyleTypeRules
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="implementation"/> implements a recognized job interface.
    /// </summary>
    public static bool IsJobStyleImplementation(Type implementation)
    {
        ArgumentNullException.ThrowIfNull(implementation);
        return implementation.GetInterfaces().Any(i => IsJobStyleInterfaceName(i.Name));
    }

    /// <summary>
    /// Job interface type names recognized without taking a package reference on Quartz/Hangfire.
    /// </summary>
    public static bool IsJobStyleInterfaceName(string name)
        => name is "IJob" or "IEasyCoreJob" or "IEasyCoreHangfireJob";
}

using AspectInjector.Broker;
using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// AspectInjector aspect that applies <see cref="SaveChangesAttribute"/> after method execution.
/// </summary>
[Aspect(Scope.Global)]
public sealed class SaveChangesAspect
{
    /// <summary>
    /// Wraps the target method with SaveChanges / optional transaction.
    /// </summary>
    [Advice(Kind.Around, Targets = Target.Method)]
    public object? Handle(
        [Argument(Source.Instance)] object instance,
        [Argument(Source.Metadata)] MethodBase method,
        [Argument(Source.Target)] Func<object[], object> target,
        [Argument(Source.Arguments)] object[] args,
        [Argument(Source.ReturnType)] Type returnType,
        [Argument(Source.Triggers)] Attribute[] triggers)
    {
        var attribute = triggers.OfType<SaveChangesAttribute>().FirstOrDefault()
                        ?? method.GetCustomAttribute<SaveChangesAttribute>(inherit: true)
                        ?? method.DeclaringType?.GetCustomAttribute<SaveChangesAttribute>(inherit: true);

        if (attribute is null)
            return target(args);

        return SaveChangesExecutor.Execute(instance, method, args, target, returnType, attribute);
    }
}

using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Resolves <see cref="SaveChangesAttribute"/> from interface / class / method placement.
/// Most-specific wins: implementation method → interface method → class → interface type.
/// </summary>
internal static class SaveChangesAttributeLocator
{
    public static SaveChangesAttribute? Find(
        Type targetType,
        MethodInfo method,
        MethodInfo? proxyMethod = null)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(method);

        var onMethod = method.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
        if (onMethod is not null)
            return onMethod;

        foreach (var iface in targetType.GetInterfaces())
        {
            var interfaceMethod = ResolveInterfaceMethod(targetType, iface, method, proxyMethod);
            if (interfaceMethod is null)
                continue;

            var onInterfaceMethod = interfaceMethod.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
            if (onInterfaceMethod is not null)
                return onInterfaceMethod;
        }

        var onClass = targetType.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
        if (onClass is not null)
            return onClass;

        foreach (var iface in targetType.GetInterfaces())
        {
            var interfaceMethod = ResolveInterfaceMethod(targetType, iface, method, proxyMethod);
            if (interfaceMethod is null)
                continue;

            var onInterfaceType = iface.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
            if (onInterfaceType is not null)
                return onInterfaceType;
        }

        return null;
    }

    public static bool IsInstrumented(Type type)
    {
        if (type.GetCustomAttribute<SaveChangesAttribute>(inherit: true) is not null)
            return true;

        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        if (type.GetMethods(flags).Any(m => m.IsDefined(typeof(SaveChangesAttribute), inherit: true)))
            return true;

        if (!type.IsInterface)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (IsInstrumented(iface))
                    return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<SaveChangesAttribute> GetInterfaceAttributes(
        Type targetType,
        MethodInfo method)
    {
        var list = new List<SaveChangesAttribute>();

        foreach (var iface in targetType.GetInterfaces())
        {
            var interfaceMethod = ResolveInterfaceMethod(targetType, iface, method, proxyMethod: null);
            if (interfaceMethod is null)
                continue;

            var onType = iface.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
            if (onType is not null)
                list.Add(onType);

            var onMethod = interfaceMethod.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
            if (onMethod is not null)
                list.Add(onMethod);
        }

        return list;
    }

    private static MethodInfo? ResolveInterfaceMethod(
        Type targetType,
        Type iface,
        MethodInfo method,
        MethodInfo? proxyMethod)
    {
        if (proxyMethod is not null && proxyMethod.DeclaringType == iface)
            return proxyMethod;

        if (method.DeclaringType == iface)
            return method;

        try
        {
            var map = targetType.GetInterfaceMap(iface);
            for (var i = 0; i < map.TargetMethods.Length; i++)
            {
                if (map.TargetMethods[i] == method || MethodsEqual(map.TargetMethods[i], method))
                    return map.InterfaceMethods[i];
            }
        }
        catch (ArgumentException)
        {
            // Interface not mapped (e.g. some DispatchProxy generated types).
        }

        foreach (var candidate in iface.GetMethods())
        {
            if (MethodsEqual(candidate, method) ||
                (proxyMethod is not null && MethodsEqual(candidate, proxyMethod)))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool MethodsEqual(MethodInfo left, MethodInfo right)
    {
        if (left == right)
            return true;

        if (!string.Equals(left.Name, right.Name, StringComparison.Ordinal))
            return false;

        var a = left.GetParameters();
        var b = right.GetParameters();
        if (a.Length != b.Length)
            return false;

        for (var i = 0; i < a.Length; i++)
        {
            if (a[i].ParameterType != b[i].ParameterType)
                return false;
        }

        return true;
    }
}

using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Castle DynamicProxy interceptor that applies <see cref="SaveChangesAttribute"/> after method execution.
/// </summary>
public sealed class SaveChangesAsyncInterceptor : IAsyncInterceptor
{
    private readonly IServiceProvider _services;

    public SaveChangesAsyncInterceptor(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        var returnType = (invocation.MethodInvocationTarget ?? invocation.Method).ReturnType;
        if (returnType == typeof(ValueTask))
        {
            invocation.ReturnValue = new ValueTask(InterceptAsync(invocation));
            return;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var resultType = returnType.GenericTypeArguments[0];
            var generic = typeof(SaveChangesAsyncInterceptor)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m => m.Name == nameof(InterceptAsync) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(resultType);
            var task = generic.Invoke(this, new object[] { invocation })!;
            invocation.ReturnValue = Activator.CreateInstance(returnType, task);
            return;
        }

        if (ShouldSkip(invocation))
        {
            invocation.Proceed();
            return;
        }

        var attribute = FindAttribute(invocation);
        if (attribute is null)
        {
            invocation.Proceed();
            return;
        }

        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        Func<object[], object> target = _ =>
        {
            invocation.Proceed();
            return invocation.ReturnValue!;
        };

        invocation.ReturnValue = SaveChangesExecutor.Execute(
            invocation.InvocationTarget ?? invocation.Proxy,
            method,
            invocation.Arguments,
            target,
            method.ReturnType,
            attribute,
            _services);
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InterceptAsync(invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        invocation.ReturnValue = InterceptAsync<TResult>(invocation);
    }

    private async Task InterceptAsync(IInvocation invocation)
    {
        await InterceptAsync<object>(invocation).ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsync<TResult>(IInvocation invocation)
    {
        if (ShouldSkip(invocation))
        {
            invocation.Proceed();
            return await UnpackAsync<TResult>(invocation.ReturnValue).ConfigureAwait(false);
        }

        var attribute = FindAttribute(invocation);
        if (attribute is null)
        {
            invocation.Proceed();
            return await UnpackAsync<TResult>(invocation.ReturnValue).ConfigureAwait(false);
        }

        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        Func<object[], object> target = _ =>
        {
            invocation.Proceed();
            return invocation.ReturnValue!;
        };

        var result = SaveChangesExecutor.Execute(
            invocation.InvocationTarget ?? invocation.Proxy,
            method,
            invocation.Arguments,
            target,
            method.ReturnType,
            attribute,
            _services);

        return await UnpackAsync<TResult>(result).ConfigureAwait(false);
    }

    private static bool ShouldSkip(IInvocation invocation)
    {
        var target = invocation.InvocationTarget ?? invocation.Proxy;
        if (target is ControllerBase)
            return true;

        var declaring = (invocation.MethodInvocationTarget ?? invocation.Method).DeclaringType;
        return declaring is not null && typeof(ControllerBase).IsAssignableFrom(declaring);
    }

    private static SaveChangesAttribute? FindAttribute(IInvocation invocation)
    {
        var targetType = invocation.TargetType
                         ?? invocation.InvocationTarget?.GetType()
                         ?? invocation.Proxy.GetType();
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        return SaveChangesAttributeLocator.Find(targetType, method, invocation.Method);
    }

    private static async Task<TResult> UnpackAsync<TResult>(object? invoked)
    {
        if (invoked is Task<TResult> typed)
            return await typed.ConfigureAwait(false);

        if (invoked is Task task)
        {
            await task.ConfigureAwait(false);
            return default!;
        }

        if (invoked is ValueTask<TResult> valueTaskTyped)
            return await valueTaskTyped.ConfigureAwait(false);

        if (invoked is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
            return default!;
        }

        return invoked is TResult direct ? direct : default!;
    }
}

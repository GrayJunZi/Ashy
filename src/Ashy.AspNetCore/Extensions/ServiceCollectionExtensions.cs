using Ashy.Context;
using Ashy.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Ashy.AspNetCore.Extensions;

/// <summary>
/// IServiceCollection 扩展，注册 Ashy 核心服务
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Ashy 核心 DI 服务（IExceptionMapper、ICorrelationContextAccessor）
    /// </summary>
    public static IServiceCollection AddAshy(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionMapper, DefaultExceptionMapper>();
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        return services;
    }
}
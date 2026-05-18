using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ashy.Modules;

/// <summary>
/// 从指定程序集扫描 IModule 实现，按 Order 排序后依次加载。
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// 扫描 assemblies 中所有 IModule 实现，按 Order 升序执行 ConfigureServices
    /// </summary>
    public static IServiceCollection LoadModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Order);

        foreach (var module in modules)
        {
            module.ConfigureServices(services, configuration);
        }

        return services;
    }

    /// <summary>
    /// 按 Order 升序执行 Configure
    /// </summary>
    public static IApplicationBuilder UseModules(
        this IApplicationBuilder app,
        params Assembly[] assemblies)
    {
        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Order);

        foreach (var module in modules)
        {
            module.Configure(app);
        }

        return app;
    }
}
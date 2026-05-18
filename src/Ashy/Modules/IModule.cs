using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ashy.Modules;

/// <summary>
/// 模块化插件接口，各功能包实现此接口以自动注册
/// </summary>
public interface IModule
{
    /// <summary>
    /// 模块唯一名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 加载顺序，数字越小越先加载，默认 0
    /// </summary>
    int Order => 0;

    /// <summary>
    /// 注册服务到 DI 容器
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// 配置中间件管道
    /// </summary>
    void Configure(IApplicationBuilder app);
}
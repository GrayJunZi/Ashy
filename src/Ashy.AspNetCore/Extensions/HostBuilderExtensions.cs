using Ashy.Abstractions;
using Ashy.Hosting;
using Ashy.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ashy.AspNetCore.Extensions;

/// <summary>
/// WebApplicationBuilder 扩展，启动 Ashy 构建管道
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// 启动 Ashy 构建管道
    /// </summary>
    public static IAshyHostBuilder AddAshy(this WebApplicationBuilder builder, Action<AshyOptions> configure)
    {
        var options = new AshyOptions();
        configure?.Invoke(options);

        if (configure is not null)
        {
            builder.Services.Configure(configure);
        }

        return new AshyHostBuilder(builder);
    }
}
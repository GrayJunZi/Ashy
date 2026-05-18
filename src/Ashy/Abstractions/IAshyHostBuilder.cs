using Ashy.Options;
using Microsoft.AspNetCore.Builder;

namespace Ashy.Abstractions;

/// <summary>
/// 链式构建器，统一注册所有 Ashy 模块。
/// 内部装配到 WebApplicationBuilder。
/// </summary>
public interface IAshyHostBuilder
{
    /// <summary>
    /// 内部持有的 WebApplicationBuilder 实例
    /// </summary>
    WebApplicationBuilder InnerBuilder { get; }

    /// <summary>注册 Nacos 模块</summary>
    IAshyHostBuilder UseNacos(Action<NacosOptions> configure);
    /// <summary>注册服务代理模块</summary>
    IAshyHostBuilder UseServiceProxy(Action<ServiceProxyOptions> configure);
    /// <summary>注册认证模块</summary>
    IAshyHostBuilder UseAuth(Action<AuthOptions> configure);
    /// <summary>注册可观测性模块</summary>
    IAshyHostBuilder UseObservability(Action<ObservabilityOptions> configure);
    /// <summary>注册消息传递模块</summary>
    IAshyHostBuilder UseMessaging(Action<MessagingOptions> configure);
    /// <summary>注册 Saga 模块</summary>
    IAshyHostBuilder UseSaga(Action<SagaOptions> configure);
    /// <summary>注册缓存模块</summary>
    IAshyHostBuilder UseCaching(Action<CachingOptions> configure);
    /// <summary>注册多租户模块</summary>
    IAshyHostBuilder UseMultiTenancy(Action<MultiTenancyOptions> configure);
    /// <summary>注册后台任务模块</summary>
    IAshyHostBuilder UseBackgroundJobs(Action<BackgroundJobOptions> configure);

    /// <summary>
    /// 完成装配，返回 WebApplication
    /// </summary>
    WebApplication Build();
}
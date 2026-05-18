using Ashy.Abstractions;
using Ashy.Options;
using Microsoft.AspNetCore.Builder;

namespace Ashy.Hosting;

/// <summary>
/// Ashy 链式构建器实现
/// </summary>
public class AshyHostBuilder : IAshyHostBuilder
{
    /// <inheritdoc />
    public WebApplicationBuilder InnerBuilder { get; }

    /// <summary>
    /// 初始化 AshyHostBuilder
    /// </summary>
    public AshyHostBuilder(WebApplicationBuilder builder)
    {
        InnerBuilder = builder;
    }

    /// <inheritdoc />
    public IAshyHostBuilder UseNacos(Action<NacosOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseServiceProxy(Action<ServiceProxyOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseAuth(Action<AuthOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseObservability(Action<ObservabilityOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseMessaging(Action<MessagingOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseSaga(Action<SagaOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseCaching(Action<CachingOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseMultiTenancy(Action<MultiTenancyOptions> configure) => this;
    /// <inheritdoc />
    public IAshyHostBuilder UseBackgroundJobs(Action<BackgroundJobOptions> configure) => this;

    /// <inheritdoc />
    public WebApplication Build()
    {
        return InnerBuilder.Build();
    }
}
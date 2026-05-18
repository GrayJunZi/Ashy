namespace Ashy.Options;

/// <summary>
/// Ashy 全局选项
/// </summary>
public class AshyOptions
{
    /// <summary>
    /// 服务名称（建议与 Nacos 注册名一致）
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 运行环境标识
    /// </summary>
    public string Environment { get; set; } = "development";
}

/// <summary>Nacos 配置桩（P0 预留）</summary>
public class NacosOptions { }

/// <summary>服务代理配置桩（P0 预留）</summary>
public class ServiceProxyOptions { }

/// <summary>认证配置桩（P0 预留）</summary>
public class AuthOptions { }

/// <summary>可观测性配置桩（P0 预留）</summary>
public class ObservabilityOptions { }

/// <summary>消息传递配置桩（P0 预留）</summary>
public class MessagingOptions { }

/// <summary>Saga 配置桩（P0 预留）</summary>
public class SagaOptions { }

/// <summary>缓存配置桩（P0 预留）</summary>
public class CachingOptions { }

/// <summary>多租户配置桩（P0 预留）</summary>
public class MultiTenancyOptions { }

/// <summary>后台任务配置桩（P0 预留）</summary>
public class BackgroundJobOptions { }
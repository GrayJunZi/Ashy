# Spec: Ashy.Core — P0 基础建设

> 对应路线图 Phase P0 (Month 1-2) | 状态：已完成 ✅

## 1. 目标

完成 Ashy 核心库的基础设施层，为所有后续模块提供：链式 Host 构建器、模块化插件系统、上下文传播、统一响应模型、全局异常处理和结构化日志抽象。

## 2. 已实现

| 组件 | 位置 | 状态 |
|------|------|:---:|
| `CorrelationContext` | `Context/CorrelationContext.cs` | ✅ |
| `ICorrelationContextAccessor` | `Context/ICorrelationContextAccessor.cs` | ✅ |
| `CorrelationContextAccessor` | `Context/ICorrelationContextAccessor.cs` | ✅ |
| `ApiResult<T>` | `Models/ApiResult.cs` | ✅ |
| `PagedResult<T>` | `Models/ApiResult.cs` | ✅ |
| `IExceptionMapper` | `Exceptions/IExceptionMapper.cs` | ✅ |
| `DefaultExceptionMapper` | `Exceptions/DefaultExceptionMapper.cs` | ✅ |
| `ProblemDetail` | `Exceptions/ProblemDetail.cs` | ✅ |
| `JsonExtensions` | `Serialization/JsonExtensions.cs` | ✅ |
| `XmlExtensions` | `Serialization/XmlExtensions.cs` | ✅ |
| `IAshyHostBuilder` | `Abstractions/IAshyHostBuilder.cs` | ✅ |
| `AshyHostBuilder` | `Hosting/AshyHostBuilder.cs` | ✅ |
| `IModule`（增强） | `Modules/IModule.cs` | ✅ |
| `ModuleLoader` | `Modules/ModuleLoader.cs` | ✅ |
| `AshyOptions` | `Options/AshyOptions.cs` | ✅ |
| `NacosOptions` 等 8 个桩 | `Options/AshyOptions.cs` | ✅ |
| `IAshyLogger`（桩） | `Logging/IAshyLogger.cs` | ✅ |
| `ExceptionHandlingMiddleware` | `Ashy.AspNetCore/Middleware/ExceptionHandlingMiddleware.cs` | ✅ |
| `HostBuilderExtensions` | `Ashy.AspNetCore/Extensions/HostBuilderExtensions.cs` | ✅ |
| `ServiceCollectionExtensions`（增强） | `Ashy.AspNetCore/Extensions/ServiceCollectionExtensions.cs` | ✅ |
| `ApplicationBuilderExtensions`（增强） | `Ashy.AspNetCore/Extensions/ApplicationBuilderExtensions.cs` | ✅ |

## 3. 实现细节（参考）

> 以下为实现时遵循的接口签名，实际代码已按此完成。

### 3.1 IAshyHostBuilder — 链式构建器

**文件**: `src/Ashy/Abstractions/IAshyHostBuilder.cs`

```csharp
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

    // 每个 UseXxx 方法返回 IAshyHostBuilder 以支持链式调用
    IAshyHostBuilder UseNacos(Action<NacosOptions> configure);
    IAshyHostBuilder UseServiceProxy(Action<ServiceProxyOptions> configure);
    IAshyHostBuilder UseAuth(Action<AuthOptions> configure);
    IAshyHostBuilder UseObservability(Action<ObservabilityOptions> configure);
    IAshyHostBuilder UseMessaging(Action<MessagingOptions> configure);
    IAshyHostBuilder UseSaga(Action<SagaOptions> configure);
    IAshyHostBuilder UseCaching(Action<CachingOptions> configure);
    IAshyHostBuilder UseMultiTenancy(Action<MultiTenancyOptions> configure);
    IAshyHostBuilder UseBackgroundJobs(Action<BackgroundJobOptions> configure);

    /// <summary>
    /// 完成装配，返回 WebApplication
    /// </summary>
    WebApplication Build();
}
```

**构建器实现**:

**文件**: `src/Ashy/Hosting/AshyHostBuilder.cs`

```csharp
namespace Ashy.Hosting;

public class AshyHostBuilder : IAshyHostBuilder
{
    public WebApplicationBuilder InnerBuilder { get; }

    public AshyHostBuilder(WebApplicationBuilder builder)
    {
        InnerBuilder = builder;
    }

    public IAshyHostBuilder UseNacos(Action<NacosOptions> configure)
    {
        // 后续模块实现
        return this;
    }

    // ... 其余 UseXxx 方法同模式

    public WebApplication Build()
    {
        return InnerBuilder.Build();
    }
}
```

**扩展方法入口**:

**文件**: `src/Ashy.AspNetCore/Extensions/HostBuilderExtensions.cs`

```csharp
namespace Ashy.AspNetCore.Extensions;

public static class HostBuilderExtensions
{
    /// <summary>
    /// 启动 Ashy 构建管道
    /// </summary>
    public static IAshyHostBuilder AddAshy(this WebApplicationBuilder builder, Action<AshyOptions> configure)
    {
        var options = new AshyOptions();
        configure?.Invoke(options);
        builder.Services.Configure(configure);
        return new AshyHostBuilder(builder);
    }
}
```

**Options**:

**文件**: `src/Ashy/Options/AshyOptions.cs`

```csharp
namespace Ashy.Options;

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
```

**注意**: `NacosOptions`、`ServiceProxyOptions` 等各模块的 Options 类暂不在此 spec 中定义，待对应模块开发时按需创建。P0 阶段 `UseXxx` 方法体为空桩。

### 3.2 IModule 插件系统增强

**修改文件**: `src/Ashy/Modules/IModule.cs`

将现有最小化接口增强为完整的模块生命周期接口：

```csharp
namespace Ashy.Modules;

/// <summary>
/// 模块化插件接口 — 自动扫描并加载。
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
```

### 3.3 模块扫描与自动加载

**文件**: `src/Ashy/Modules/ModuleLoader.cs`

```csharp
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
```

### 3.4 ICorrelationContextAccessor 接口

当前 `CorrelationContext` 直接暴露 `static` 属性，需要增加可注入的访问器接口。

**文件**: `src/Ashy/Context/ICorrelationContextAccessor.cs`

```csharp
namespace Ashy.Context;

/// <summary>
/// CorrelationContext 访问器，通过 DI 注入使用（便于单元测试 mock）
/// </summary>
public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; set; }
}

/// <summary>
/// 基于 AsyncLocal 的默认实现
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContextHolder> _current = new();

    public CorrelationContext? Current
    {
        get => _current.Value?.Context;
        set
        {
            var holder = _current.Value;
            if (holder is not null)
            {
                holder.Context = null;
            }

            if (value is not null)
            {
                _current.Value = new CorrelationContextHolder { Context = value };
            }
        }
    }

    private sealed class CorrelationContextHolder
    {
        public CorrelationContext? Context;
    }
}
```

**同时修改** `CorrelationContext`：移除静态属性，`Current` 改为通过 `ICorrelationContextAccessor` 获取。

**修改文件**: `src/Ashy/Context/CorrelationContext.cs`

移除：
```csharp
private static readonly AsyncLocal<CorrelationContext?> _current = new();
public static CorrelationContext? Current { get; set; }
```

### 3.5 异常处理中间件

**文件**: `src/Ashy.AspNetCore/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
namespace Ashy.AspNetCore.Middleware;

/// <summary>
/// 全局异常处理中间件：捕获未处理异常，通过 IExceptionMapper 转为 RFC 7807 ProblemDetails 响应
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionMapper _mapper;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly bool _includeDetails;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IExceptionMapper mapper,
        ILogger<ExceptionHandlingMiddleware> logger,
        bool includeDetails = false)
    {
        _next = next;
        _mapper = mapper;
        _logger = logger;
        _includeDetails = includeDetails;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var problem = _mapper.Map(ex);
            context.Response.StatusCode = problem.Status;
            context.Response.ContentType = "application/problem+json";

            var detail = _includeDetails ? problem.Detail : null;

            await context.Response.WriteAsJsonAsync(new
            {
                type = problem.Type ?? "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = problem.Title,
                status = problem.Status,
                detail,
                instance = problem.Instance ?? context.Request.Path
            });
        }
    }
}
```

**注册方法**:

**文件**: `src/Ashy.AspNetCore/Extensions/ApplicationBuilderExtensions.cs` (修改现有文件)

在现有 `UseAshy` 方法中添加中间件注册：

```csharp
public static IApplicationBuilder UseAshy(this IApplicationBuilder app, bool includeExceptionDetails = false)
{
    // 注册异常处理中间件
    app.UseMiddleware<ExceptionHandlingMiddleware>(includeExceptionDetails);
    return app;
}
```

同步修改 `ServiceCollectionExtensions.AddAshy` 注册依赖：

**文件**: `src/Ashy.AspNetCore/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddAshy(this IServiceCollection services)
{
    services.AddSingleton<IExceptionMapper, DefaultExceptionMapper>();
    services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
    return services;
}
```

### 3.6 结构化日志抽象

**文件**: `src/Ashy/Logging/IAshyLogger.cs`

暂不实现，S0 阶段由 Serilog 模块提供。此处仅定义接口：

```csharp
namespace Ashy.Logging;

/// <summary>
/// Ashy 结构化日志抽象（S0 阶段接口预留，实际日志使用 ILogger<T>）
/// 后续 Ashy.Serilog 模块将提供扩展实现
/// </summary>
public interface IAshyLogger
{
    void Trace(string message, params object?[] args);
    void Debug(string message, params object?[] args);
    void Info(string message, params object?[] args);
    void Warn(string message, params object?[] args);
    void Error(string message, params object?[] args);
    void Fatal(string message, params object?[] args);
}
```

P0 阶段不做具体实现，仅接口定义留桩。

## 4. DI 注册总览

P0 完成后，`AddAshy()` 应注册以下服务：

| 服务 | 生命周期 | 说明 |
|------|----------|------|
| `IExceptionMapper` | Singleton | 默认注册 `DefaultExceptionMapper` |
| `DefaultExceptionMapper` | Singleton | 自身也注册，允许替换 |
| `ICorrelationContextAccessor` | Singleton | 默认注册 `CorrelationContextAccessor` |

P0 完成后的中间件管道：

```
ExceptionHandlingMiddleware → (用户自定义) → 下游中间件
```

## 5. 文件变更清单

| 操作 | 文件 |
|------|------|
| 新增 | `src/Ashy/Abstractions/IAshyHostBuilder.cs` |
| 新增 | `src/Ashy/Hosting/AshyHostBuilder.cs` |
| 新增 | `src/Ashy/Options/AshyOptions.cs` |
| 新增 | `src/Ashy/Options/NacosOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/ServiceProxyOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/AuthOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/ObservabilityOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/MessagingOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/SagaOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/CachingOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/MultiTenancyOptions.cs`（桩） |
| 新增 | `src/Ashy/Options/BackgroundJobOptions.cs`（桩） |
| 新增 | `src/Ashy/Logging/IAshyLogger.cs` |
| 新增 | `src/Ashy/Modules/ModuleLoader.cs` |
| 新增 | `src/Ashy/Context/ICorrelationContextAccessor.cs` |
| 新增 | `src/Ashy.AspNetCore/Middleware/ExceptionHandlingMiddleware.cs` |
| 新增 | `src/Ashy.AspNetCore/Extensions/HostBuilderExtensions.cs` |
| 修改 | `src/Ashy/Modules/IModule.cs`（增加 ConfigureServices/Configure） |
| 修改 | `src/Ashy/Context/CorrelationContext.cs`（移除 static AsyncLocal） |
| 修改 | `src/Ashy.AspNetCore/Extensions/ServiceCollectionExtensions.cs`（注册服务） |
| 修改 | `src/Ashy.AspNetCore/Extensions/ApplicationBuilderExtensions.cs`（注册中间件） |

## 6. 测试用例骨架

### 6.1 CorrelationContext 测试

```csharp
// 1. 默认构造应自动生成 TraceId (32 hex), SpanId (16 hex)
// 2. FromTraceParent 应正确解析 W3C traceparent 字符串
// 3. Create(tenantId, userId) 应设置对应属性
// 4. TraceParent 属性应返回 "00-{TraceId}-{SpanId}-{TraceFlags}" 格式
// 5. Items 字典初始为空，可读写
```

### 6.2 ExceptionMapper 测试

```csharp
// 1. ArgumentException → 400 (Status=400, Title 含 "Bad Request")
// 2. UnauthorizedAccessException → 401
// 3. InvalidOperationException → 409
// 4. NotImplementedException → 501
// 5. 未知异常 → 500
// 6. ProblemDetail.Type 应为对应 RFC URI
```

### 6.3 ModuleLoader 测试

```csharp
// 1. 扫描无模块程序集 → 不抛异常
// 2. 扫描含单个 IModule 程序集 → ConfigureServices 被调用一次
// 3. 扫描含多个 IModule 程序集 → 按 Order 排序后依次调用
// 4. 抽象类不被实例化
// 5. 接口类型不被实例化
```

### 6.4 ApiResult 测试

```csharp
// 1. ApiResult<T>.Ok(data) → Success=true, Message=null, Data=data
// 2. ApiResult<T>.Fail(msg) → Success=false, Message=msg, Data=default
// 3. PagedResult.HasNextPage / HasPrevPage / TotalPages 边界条件
```

### 6.5 AshyHostBuilder 测试

```csharp
// 1. AddAshy 返回 IAshyHostBuilder 实例
// 2. 链式调用各 UseXxx 方法不抛异常
// 3. Build() 返回 WebApplication（集成测试）
```

## 7. 验收标准

- [x] `dotnet build` 所有项目通过，零警告
- [x] `dotnet test` 全部测试通过（32 个测试用例）
- [x] `CorrelationContext` 通过 `ICorrelationContextAccessor` 可注入使用
- [x] `IModule` 接口包含 `ConfigureServices` 和 `Configure` 方法
- [x] `ModuleLoader` 可正确扫描和加载模块
- [x] `ExceptionHandlingMiddleware` 捕获未处理异常并返回 RFC 7807 格式
- [x] `AddAshy()` 注册 `IExceptionMapper` 和 `ICorrelationContextAccessor`
- [x] `UseAshy()` 注册 `ExceptionHandlingMiddleware`
- [x] `AddAshy(WebApplicationBuilder)` 链式构建器可用
- [x] 所有 Options 桩类存在且放在 `Ashy.Options` namespace

---

> 本 spec 为 P0 实现规范，后续各模块 spec 将沿用此模板。
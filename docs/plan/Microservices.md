# Ashy — .NET 10 企业级微服务框架

> 开源协议：MIT | 目标：成为 .NET 生态的全功能微服务全家桶

---

## 目录

1. [愿景与定位](#1-愿景与定位)
2. [架构全景图](#2-架构全景图)
3. [项目结构 & NuGet 包拆分](#3-项目结构--nuget-包拆分)
4. [各模块详细设计](#4-各模块详细设计)
   - [4.1 Ashy.Core — 核心基础](#41-ashycore--核心基础)
   - [4.2 Ashy.Nacos — 服务发现 & 配置中心](#42-ashynacos--服务发现--配置中心)
   - [4.3 Ashy.Gateway — API 网关](#43-ashygateway--api-网关)
   - [4.4 Ashy.ServiceProxy — 服务间通信](#44-ashyserviceproxy--服务间通信)
   - [4.5 Ashy.Auth — 认证授权](#45-ashyauth--认证授权)
   - [4.6 Ashy.Observability — 可观测性](#46-ashyobservability--可观测性)
   - [4.7 Ashy.Messaging — 事件总线](#47-ashymessaging--事件总线)
   - [4.8 Ashy.Saga — 分布式事务](#48-ashysaga--分布式事务)
   - [4.9 Ashy.Caching — 缓存](#49-ashycaching--缓存)
   - [4.10 Ashy.MultiTenancy — 多租户](#410-ashymultitenancy--多租户)
   - [4.11 Ashy.BackgroundJobs — 后台任务](#411-ashybackgroundjobs--后台任务)
   - [4.12 Ashy.DDD — 领域驱动设计构建块](#412-ashyddd--领域驱动设计构建块)
   - [4.13 Ashy.Dapr — Dapr 集成（可选）](#413-ashydapr--dapr-集成可选)
5. [技术栈选型](#5-技术栈选型)
6. [分阶段路线图](#6-分阶段路线图)
7. [开源社区运营策略](#7-开源社区运营策略)
8. [关键风险与缓解](#8-关键风险与缓解)

---

## 1. 愿景与定位

**Ashy** 是一个面向 .NET 10 的企业级微服务框架，提供从服务注册发现、配置管理、API 网关、服务间通信、认证授权、可观测性、事件总线、分布式事务、缓存、多租户到 DDD 构建块的全套基础设施能力。

### 核心设计原则

- **开箱即用**：每个模块独立可插拔，一行代码启用功能
- **约定优于配置**：合理的默认值，允许深度定制
- **性能优先**：基于 Minimal API、gRPC 高性能通道、Native AOT 友好
- **弹性至上**：内置断路器、重试、超时、舱壁等弹性策略
- **可观测性内建**：OpenTelemetry 全链路追踪、指标、日志开箱即用
- **生态友好**：可选集成 Dapr，与现有 .NET 生态无缝衔接

### 与竞品的差异化

| 对比维度 | Ashy | Steeltoe | ABP Framework |
|----------|------|----------|---------------|
| .NET 版本 | .NET 10 原生 | 适配多版本 | 适配多版本 |
| 注册中心 | Nacos 优先 | Consul/Eureka | 无内置 |
| 配置中心 | Nacos 一体化 | Spring Cloud Config | 自研 |
| API 风格 | Minimal API 优先 | Controller | Controller |
| 网关 | YARP 动态路由 | 无内置 | 无内置 |
| Native AOT | 目标支持 | 部分支持 | 不支持 |
| 协议 | HTTP REST + gRPC 双协议 | HTTP | HTTP |

---

## 2. 架构全景图

```
┌──────────────────────────────────────────────────────────────┐
│                      API Gateway                             │
│               (YARP + Dynamic Routing)                       │
├──────────────────────────────────────────────────────────────┤
│   Auth (JWT/OAuth2)  │  Rate Limiting  │  Transformation     │
├──────────────────────────────────────────────────────────────┤
│                Service Communication Layer                   │
│       HTTP(REST) ⬦ gRPC  │  Service Discovery  │  LB        │
├──────────────────────────────────────────────────────────────┤
│   Resilience Pipeline                                        │
│   Circuit Breaker │ Retry │ Timeout │ Bulkhead               │
├──────────────────────────────────────────────────────────────┤
│                    Service Layer                             │
│                                                              │
│   ┌─────────────────────────┐   ┌─────────────────────────┐ │
│   │   Business Services     │   │  Infrastructure Services │ │
│   │                         │   │                         │ │
│   │   - OrderService        │   │  ┌───────────────────┐  │ │
│   │   - PaymentService      │   │  │ Nacos             │  │ │
│   │   - InventoryService    │   │  │ (Register/Config) │  │ │
│   │   - UserService         │   │  ├───────────────────┤  │ │
│   │                         │   │  │ Caching (Redis)   │  │ │
│   │                         │   │  ├───────────────────┤  │ │
│   │                         │   │  │ Event Bus         │  │ │
│   │                         │   │  │ (RabbitMQ/Kafka)  │  │ │
│   │                         │   │  ├───────────────────┤  │ │
│   │                         │   │  │ Saga Manager      │  │ │
│   │                         │   │  ├───────────────────┤  │ │
│   │                         │   │  │ Background Jobs   │  │ │
│   │                         │   │  ├───────────────────┤  │ │
│   │                         │   │  │ Multi-Tenancy     │  │ │
│   │                         │   │  └───────────────────┘  │ │
│   └─────────────────────────┘   └─────────────────────────┘ │
├──────────────────────────────────────────────────────────────┤
│                     Observability Layer                      │
│    OpenTelemetry │ Metrics (Prometheus) │ Tracing │ Logging  │
├──────────────────────────────────────────────────────────────┤
│             Optional: Dapr Integration Layer                 │
│    Pub/Sub Adapter │ State Store │ Secret Store │ Actor      │
└──────────────────────────────────────────────────────────────┘
```

### 请求生命周期

```
外部请求
  │
  ▼
API Gateway (YARP)
  ├── 认证中间件（JWT 验证）
  ├── 限流中间件
  ├── 租户解析中间件
  │
  ▼
目标微服务
  ├── 中间件管道
  │   ├── 租户上下文注入
  │   ├── 链路追踪 (OpenTelemetry)
  │   └── 异常处理
  │
  ├── 业务逻辑层
  │   ├── DDD 构建块 (AggregateRoot / DomainEvent)
  │   ├── 仓储模式 (Repository)
  │   └── 工作单元 (Unit of Work)
  │
  ├── 基础设施层
  │   ├── Ashy.ServiceProxy → 调用下游服务
  │   ├── Ashy.Messaging → 发布领域事件
  │   ├── Ashy.Caching → 缓存读写
  │   └── Ashy.Saga → 执行分布式事务
  │
  └── 响应返回
      └── 统一响应模型 (ApiResult<T>)
```

---

## 3. 项目结构 & NuGet 包拆分

```
ashy/
├── src/
│   │
│   ├── Ashy.Core/                        # ⭐ 核心抽象层
│   │   ├── Abstractions/                 # 所有核心接口定义
│   │   ├── Hosting/                      # IAshyHostBuilder 链式构建器
│   │   ├── Modules/                      # IModule 模块化插件系统
│   │   ├── Context/                      # ICorrelationContext 上下文传播
│   │   ├── Models/                       # ApiResult<T>, PagedResult<T>
│   │   ├── Exceptions/                   # 全局异常处理抽象
│   │   └── Extensions/                   # IServiceCollection 扩展方法
│   │
│   ├── Ashy.Nacos/                       # Nacos 服务发现 + 配置中心
│   │   ├── Discovery/                    # 服务注册、发现、健康检查
│   │   ├── Configuration/                # 配置拉取、热刷新、监听
│   │   └── Extensions/
│   │
│   ├── Ashy.Gateway/                     # API 网关
│   │   ├── Routing/                      # 动态路由同步
│   │   ├── Middleware/                   # 限流/认证/转换/聚合
│   │   ├── Management/                   # 管理 API
│   │   └── Grpc/                         # gRPC-Web 转码
│   │
│   ├── Ashy.ServiceProxy/                # 服务间通信
│   │   ├── Http/                         # HTTP 客户端代理
│   │   ├── Grpc/                         # gRPC 客户端代理
│   │   ├── LoadBalancing/                # 负载均衡策略
│   │   ├── Resilience/                   # Polly 弹性管道
│   │   └── Discovery/                    # 服务发现解析器
│   │
│   ├── Ashy.Auth/                        # 认证授权抽象
│   ├── Ashy.Auth.Jwt/                    # JWT 实现
│   ├── Ashy.Auth.OAuth2/                 # OAuth2 实现
│   │
│   ├── Ashy.Observability/               # 可观测性
│   │   ├── Tracing/                      # OpenTelemetry Tracing
│   │   ├── Metrics/                      # Prometheus Metrics
│   │   ├── Logging/                      # Serilog 集成
│   │   ├── HealthChecks/                 # 健康检查扩展
│   │   └── Dashboards/                   # Grafana 仪表盘 JSON
│   │
│   ├── Ashy.Messaging/                   # 事件总线抽象
│   ├── Ashy.Messaging.RabbitMQ/          # RabbitMQ 实现
│   ├── Ashy.Messaging.Kafka/             # Kafka 实现
│   ├── Ashy.Messaging.InMemory/          # 内存实现（开发/测试）
│   │
│   ├── Ashy.Saga/                        # Saga 分布式事务
│   │   ├── Orchestration/                # 编排式 Saga 引擎
│   │   ├── Persistence/                  # 状态持久化
│   │   └── Extensions/
│   │
│   ├── Ashy.Caching/                     # 缓存抽象
│   ├── Ashy.Caching.Redis/               # Redis 实现
│   │
│   ├── Ashy.MultiTenancy/                # 多租户
│   │   ├── Resolution/                   # 租户解析策略
│   │   ├── Context/                      # 租户上下文传播
│   │   ├── DataIsolation/                # 数据隔离策略
│   │   └── Extensions/
│   │
│   ├── Ashy.BackgroundJobs/              # 后台任务
│   │   ├── Scheduling/                   # Quartz.NET 集成
│   │   ├── Management/                   # 任务管理 API
│   │   └── Extensions/
│   │
│   ├── Ashy.DDD/                         # DDD 战术设计构建块
│   │   ├── Entities/                     # Entity, AggregateRoot
│   │   ├── ValueObjects/                 # ValueObject 基类
│   │   ├── DomainEvents/                 # IDomainEvent 分发器
│   │   ├── Specifications/               # ISpecification<T>
│   │   ├── Repositories/                 # IRepository<T>
│   │   └── UnitOfWork/                   # IUnitOfWork
│   │
│   └── Ashy.Dapr/                        # Dapr 集成（可选）
│       ├── PubSub/                       # Dapr PubSub → IEventBus
│       ├── StateStore/                   # Dapr State → ICacheProvider
│       └── Actors/                       # Actor 模式封装
│
├── samples/
│   ├── EShopOnAshy/                      # 完整电商示例
│   │   ├── Ashy.Gateway/
│   │   ├── Services/
│   │   │   ├── CatalogService/
│   │   │   ├── OrderService/
│   │   │   ├── PaymentService/
│   │   │   └── UserService/
│   │   └── docker-compose.yml
│   │
│   └── BasicMicroservice/                # 最小化入门示例
│       ├── ServiceA/
│       └── ServiceB/
│
├── tests/
│   ├── Ashy.Core.Tests/
│   ├── Ashy.Nacos.Tests/
│   ├── Ashy.ServiceProxy.Tests/
│   ├── Ashy.Messaging.Tests/
│   ├── Ashy.Saga.Tests/
│   └── Ashy.IntegrationTests/            # 端到端集成测试
│
├── benchmarks/
│   └── Ashy.Benchmarks/                  # BenchmarkDotNet 性能测试
│
├── docs/                                 # 文档站点（Docusaurus）
│   ├── getting-started/
│   ├── modules/
│   ├── architecture/
│   ├── samples/
│   └── adr/                              # 架构决策记录
│
├── build/
│   ├── build.ps1                         # 构建脚本
│   ├── pack.ps1                          # 打包脚本
│   └── publish.ps1                       # 发布脚本
│
├── Directory.Packages.props              # 中央包管理
├── Directory.Build.props                 # 公共 MSBuild 属性
├── Ashy.sln
├── README.md
├── CONTRIBUTING.md
├── LICENSE                                # MIT License
└── CHANGELOG.md
```

---

## 4. 各模块详细设计

### 4.1 Ashy.Core — 核心基础

**定位**：框架基石，所有其他模块的共同依赖。提供统一的 DI 注册模式、Host 构建管道、模块化装配和上下文传播。

#### 核心抽象

```csharp
// === Host 构建器 ===
public interface IAshyHostBuilder
{
    IAshyHostBuilder UseNacos(Action<NacosOptions> configure);
    IAshyHostBuilder UseGateway(Action<GatewayOptions> configure);
    IAshyHostBuilder UseServiceProxy(Action<ServiceProxyOptions> configure);
    IAshyHostBuilder UseAuth(Action<AuthOptions> configure);
    IAshyHostBuilder UseObservability(Action<ObservabilityOptions> configure);
    IAshyHostBuilder UseMessaging(Action<MessagingOptions> configure);
    IAshyHostBuilder UseSaga(Action<SagaOptions> configure);
    IAshyHostBuilder UseCaching(Action<CachingOptions> configure);
    IAshyHostBuilder UseMultiTenancy(Action<MultiTenancyOptions> configure);
    IAshyHostBuilder UseBackgroundJobs(Action<BackgroundJobOptions> configure);
    IAshyHostBuilder UseDDD(Action<DDDOptions> configure);
    IAshyHostBuilder UseDapr(Action<DaprOptions> configure);
    WebApplication Build();
}

// === 模块化插件 ===
public interface IModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void Configure(IApplicationBuilder app);
}

// === 上下文传播 ===
public interface ICorrelationContext
{
    string TraceId { get; }
    string SpanId { get; }
    string? TenantId { get; }
    string? UserId { get; }
    IDictionary<string, string> Items { get; }
}

// === 统一响应模型 ===
public record ApiResult<T>(bool Success, string? Message, T? Data);
public record PagedResult<T>(int Page, int PageSize, long Total, IEnumerable<T> Items);
```

#### 功能清单

| 功能 | 说明 |
|------|------|
| `IAshyHostBuilder` | 链式构建器，统一注册所有模块，内部装配到 `WebApplicationBuilder` |
| `IModule` 插件系统 | 自动扫描并加载模块，支持启用/禁用 |
| DI 增强 | 自动注册（扫描程序集）、装饰器模式支持、Keyed Service |
| `ICorrelationContext` | TraceId/SpanId/TenantId/UserId 统一上下文，通过 AsyncLocal 传播 |
| `ApiResult<T>` | 统一响应格式，支持成功/失败/分页 |
| 全局异常处理 | `IExceptionMapper` 将异常映射为 ProblemDetails (RFC 7807) |
| 日志抽象 | 结构化日志接口，Serilog 为默认实现 |
| 配置绑定 | 强类型 Options 模式，支持 Nacos 动态刷新 |

#### 设计要点

- **零依赖**：Core 层不依赖任何外部基础设施，仅定义接口和默认实现
- **AsyncLocal 传播**：`ICorrelationContext` 使用 `AsyncLocal<T>` 实现，自动跟随 `async/await` 上下文，不依赖 `HttpContext`
- **Option 验证**：所有 Options 类使用 `[ValidateDataAnnotations]` 进行启动时校验

---

### 4.2 Ashy.Nacos — 服务发现 & 配置中心

**定位**：统一的服务注册发现与配置管理模块。基于阿里巴巴 Nacos，一个组件解决两个核心需求。

#### 架构设计

```
┌──────────────────────────────────────┐
│            Ashy.Nacos                │
│                                      │
│  ┌────────────────────────────────┐  │
│  │    NacosServiceRegistry        │  │  服务注册
│  │    - RegisterAsync()           │  │
│  │    - DeregisterAsync()         │  │
│  │    - HeartbeatAsync()          │  │
│  └────────────────────────────────┘  │
│                                      │
│  ┌────────────────────────────────┐  │
│  │    NacosServiceDiscovery       │  │  服务发现
│  │    - GetServicesAsync()        │  │
│  │    - GetInstancesAsync()       │  │
│  │    - WatchServiceAsync()       │  │
│  └────────────────────────────────┘  │
│                                      │
│  ┌────────────────────────────────┐  │
│  │    NacosConfigurationProvider  │  │  配置中心
│  │    - Load() 配置加载           │  │
│  │    - Set()  配置热刷新         │  │
│  │    - Watch() 配置监听          │  │
│  └────────────────────────────────┘  │
│                                      │
│  ┌────────────────────────────────┐  │
│  │    NacosHealthCheckPublisher   │  │  健康检查
│  │    - 集成 K8s/Consul/Nacos     │  │
│  └────────────────────────────────┘  │
└──────────────────────────────────────┘
```

#### API 设计

```csharp
// 注册服务
app.UseNacos(nacos => {
    nacos.ServerAddresses = new[] { "http://localhost:8848" };
    nacos.Namespace = "production";
    nacos.Group = "ashy";
    nacos.ServiceName = "order-service";
    nacos.ClusterName = "DEFAULT";
    nacos.Weight = 1.0f;
    nacos.Metadata = new Dictionary<string, string> {
        { "version", "1.0.0" },
        { "region", "cn-hangzhou" }
    };
});

// 使用配置中心
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddNacos(config => {
    config.DataId = "order-service";
    config.Group = "ashy";
    config.Tenant = "production";
    config.Listeners = new[] {
        new { DataId = "shared-db-settings", Group = "common" }
    };
});

// 绑定热刷新配置
builder.Services.Configure<OrderServiceOptions>(
    builder.Configuration.GetSection("OrderService"));

// 订阅配置变更
builder.Services.AddSingleton<IConfigurationChangeNotifier, NacosChangeNotifier>();
```

#### 功能清单

| 功能 | 说明 |
|------|------|
| 服务注册 | 启动时自动注册到 Nacos，支持 Metadata / Weight / Cluster |
| 服务发现 | 实时获取健康实例列表，支持按 Cluster/Group/Namespace 隔离 |
| 心跳上报 | 定时发送心跳保活，支持 gRPC 长连接模式 |
| 优雅下线 | 收到 SIGTERM 时主动反注册，确保流量摘除后再停止 |
| 配置加载 | 支持多 DataId、多 Group，优先级：本地 < 远程 < 命令行 |
| 热刷新 | 基于 Nacos Watch 机制，配置变更自动推送，结合 `IOptionsSnapshot` |
| 共享配置 | 公共配置（如数据库连接字符串）独立 DataId，多服务共享 |
| 健康检查 | 与 ASP.NET Core Health Checks 集成，上报到 Nacos |

#### 配置管理优先级

```
命令行参数 > 环境变量 > Nacos 远程配置 > appsettings.json
```

#### 错误处理

- Nacos 不可用时降级为本地缓存配置，服务正常运行
- 服务发现不可用时使用本地缓存的实例列表，配合断路器熔断
- 所有 Nacos 调用内置重试策略（3 次）

---

### 4.3 Ashy.Gateway — API 网关

**定位**：基于 YARP (Yet Another Reverse Proxy) 构建的动态 API 网关，作为系统统一入口。

#### 架构设计

```
Internet
   │
   ▼
┌──────────────────────────────────────────────────┐
│              Ashy.Gateway (YARP)                 │
│                                                  │
│  ┌────────────┐ ┌──────────┐ ┌───────────────┐  │
│  │ Auth       │ │ Rate     │ │ Transform     │  │
│  │ Middleware │ │ Limiting │ │ Middleware    │  │
│  └────────────┘ └──────────┘ └───────────────┘  │
│                                                  │
│  ┌────────────────────────────────────────────┐  │
│  │         Dynamic Route Provider             │  │
│  │  Nacos → YARP RouteConfig → Proxy Forward │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  ┌────────────────────────────────────────────┐  │
│  │            gRPC-Web Transcoder             │  │
│  └────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────┘
   │
   ▼
┌──────────┐  ┌──────────┐  ┌──────────┐
│ Order    │  │ Payment  │  │ Catalog  │
│ Service  │  │ Service  │  │ Service  │
└──────────┘  └──────────┘  └──────────┘
```

#### 功能清单

| 功能 | 说明 |
|------|------|
| 动态路由 | 从 Nacos 自动拉取服务列表，实时生成 YARP 路由配置 |
| 路由策略 | 支持 Path/Header/Query/Host 匹配，权重分流 |
| 认证中继 | JWT 验证后，将 Claims 透传到下游服务 |
| 限流 | 基于 AspNetCoreRateLimit，支持按 IP/用户/租户/端点限流 |
| 请求/响应转换 | 添加/删除/修改 Header、路径重写 |
| 请求聚合 | 将多个微服务调用合并为单个响应 |
| gRPC-Web 转码 | 允许浏览器直接调用 gRPC 服务 |
| 管理 API | 运行时添加/删除路由、调整限流策略 |
| 健康检查 | 聚合下游服务健康状态 |
| CORS | 统一跨域配置 |

#### 动态路由流程

```
1. 网关启动 → 连接 Nacos
2. 订阅服务变更 → Nacos Watch API
3. 服务上线 → 获取实例列表 → 生成 RouteConfig → 注入 YARP
4. 服务下线 → 移除 RouteConfig → YARP 热更新
```

---

### 4.4 Ashy.ServiceProxy — 服务间通信

**定位**：提供服务间调用的 HTTP 和 gRPC 客户端代理，集成服务发现、负载均衡和弹性策略。

#### 架构设计

```
业务服务
   │
   ▼
┌──────────────────────────────────────────────────┐
│              Ashy.ServiceProxy                   │
│                                                  │
│  ┌────────────────────────────────────────────┐  │
│  │        Service Address Resolver            │  │
│  │   "service://order-service"               │  │
│  │        → http://10.0.1.5:8080             │  │
│  ├────────────────────────────────────────────┤  │
│  │        Load Balancer                      │  │
│  │   RoundRobin / Random / LeastConnection    │  │
│  │   / ConsistentHash                         │  │
│  ├────────────────────────────────────────────┤  │
│  │        Resilience Pipeline (Polly)         │  │
│  │   Retry → CircuitBreaker → Timeout         │  │
│  │   → Bulkhead → Fallback                    │  │
│  ├────────────────────────────────────────────┤  │
│  │        HTTP / gRPC Client                  │  │
│  │   IHttpClientFactory / gRPC Channel        │  │
│  └────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────┘
```

#### API 设计

```csharp
// === HTTP 客户端 ===
public interface IAshyHttpClient
{
    Task<TResponse> GetAsync<TResponse>(string serviceName, string path);
    Task<TResponse> PostAsync<TResponse>(string serviceName, string path, object body);
    Task<TResponse> PutAsync<TResponse>(string serviceName, string path, object body);
    Task DeleteAsync(string serviceName, string path);
}

// === gRPC 客户端 ===
// 使用自定义 Resolver，gRPC Channel 地址由服务发现解析
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("service://order-service");
});

// === 注册 ===
builder.Services.AddAshyServiceProxy(options =>
{
    options.LoadBalancing = LoadBalancingStrategy.LeastConnection;
    options.Resilience.EnableCircuitBreaker = true;
    options.Resilience.FailureThreshold = 0.5;
    options.Resilience.SamplingDuration = TimeSpan.FromSeconds(30);
    options.Resilience.BreakDuration = TimeSpan.FromSeconds(60);
    options.Resilience.RetryCount = 3;
    options.Resilience.Timeout = TimeSpan.FromSeconds(10);
});
```

#### 负载均衡策略

| 策略 | 说明 | 适用场景 |
|------|------|----------|
| RoundRobin | 轮询 | 无状态服务，实例性能相近 |
| Random | 随机 | 简单场景 |
| LeastConnection | 最少连接数 | 长连接场景 |
| ConsistentHash | 一致性哈希 | 有状态服务，需要粘性路由 |

#### 弹性策略管道

```
Request → Retry (指数退避, 3次)
       → CircuitBreaker (50% 失败率, 30s 采样, 60s 断开)
       → Timeout (10s)
       → Bulkhead (限制并发)
       → Fallback (降级响应)
```

#### 上下文传播

- HTTP: 自动注入 `X-Trace-Id`, `X-Span-Id`, `X-Tenant-Id`, `X-User-Id` Header
- gRPC: 通过 Metadata 传递 `traceparent`, `tracestate`, `ashy-tenant-id`

---

### 4.5 Ashy.Auth — 认证授权

**定位**：提供统一的认证授权抽象，支持 JWT 和 OAuth2 实现。

#### 功能清单

| 功能 | 说明 |
|------|------|
| JWT 验证 | 支持对称/非对称密钥，公钥可从 Nacos 配置中心动态刷新 |
| OAuth2 流程 | ClientCredentials / Password / AuthorizationCode 封装 |
| 令牌中继 | 服务间调用自动携带上游令牌 |
| 权限模型 | 基于策略的 RBAC，支持资源级授权 |
| 多租户集成 | 从 Token Claim 中提取 TenantId |
| 远程令牌验证 | 支持 IdentityServer / Keycloak 等外部 IDP |

#### API 设计

```csharp
builder.Services.AddAshyAuth(options =>
{
    options.UseJwt(jwt =>
    {
        jwt.Authority = "https://identity.ashy.io";
        jwt.Audience = "order-service";
        // 或手动配置密钥 + 从 Nacos 动态刷新
        jwt.IssuerSigningKeyResolver = (token, securityKey, kid, parameters) =>
        {
            // 从配置中心获取
        };
    });

    options.AddPolicy("admin", policy =>
        policy.RequireRole("admin"));

    options.AddResourcePolicy("order:write", policy =>
        policy.RequireClaim("scope", "order.write"));
});

// 使用
[Authorize("order:write")]
app.MapGet("/api/orders", async (IOrderService service) => { ... });
```

---

### 4.6 Ashy.Observability — 可观测性

**定位**：基于 OpenTelemetry 提供全链路追踪、指标采集和结构化日志的全套可观测能力。

#### 功能清单

| 功能 | 说明 |
|------|------|
| Tracing | OpenTelemetry Tracing，自动采集 HTTP/gRPC/EF Core/MQ 链路 |
| Metrics | 采集到 Prometheus，内置 Grafana 仪表盘模板 |
| Logging | Serilog 结构化日志，支持 Elasticsearch/Loki/Console |
| Context Propagation | W3C TraceContext 标准，跨服务自动传播 |
| Health Checks | Liveness/Readiness/Startup 三阶段探针，发布到 Nacos/K8s |
| Health Check UI | 可视化健康检查仪表盘 |

#### Tracing 自动采集范围

```
HTTP In/Out → gRPC In/Out → EF Core Queries → EventBus Publish/Consume
→ Redis Commands → HttpClient Calls → Background Jobs
```

#### 内置指标

```
ashy_http_requests_total          # HTTP 请求计数
ashy_http_request_duration_ms     # HTTP 请求延迟
ashy_grpc_requests_total          # gRPC 请求计数
ashy_event_published_total        # 事件发布计数
ashy_event_consumed_total         # 事件消费计数
ashy_saga_transitions_total       # Saga 状态转换计数
ashy_circuit_breaker_state        # 断路器状态
ashy_service_health_status        # 服务健康状态
```

---

### 4.7 Ashy.Messaging — 事件总线

**定位**：提供统一的事件总线抽象，支持 RabbitMQ 和 Kafka 双实现。

#### 核心抽象

```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent;

    Task SubscribeAsync<T>(Func<T, Task> handler, SubscriptionConfig config, CancellationToken ct = default)
        where T : IIntegrationEvent;
}

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
}

public record SubscriptionConfig
{
    public string QueueName { get; init; }
    public int MaxConcurrency { get; init; } = 1;
    public bool EnableDeadLetter { get; init; } = true;
    public int RetryCount { get; init; } = 3;
    public TimeSpan RetryInterval { get; init; } = TimeSpan.FromSeconds(5);
}

// === 事务性发件箱 ===
public interface ITransactionalEventBus : IEventBus
{
    Task CommitAsync(ITransaction transaction);
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public OutboxStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; }
}
```

#### RabbitMQ 实现特性

- 交换机/队列/绑定自动声明
- 消息持久化 + 手动 ACK
- 死信队列 + TTL 延迟消息
- 消费者并发控制 + 顺序消费

#### Kafka 实现特性

- 分区策略（Key-based / RoundRobin）
- 消费者组管理
- At-least-once + Outbox 实现 Exactly-once
- Schema Registry 集成（可选）

#### 事务性发件箱 (Outbox Pattern)

```
┌──────────────────────────────────────┐
│  数据库事务                           │
│  ├── INSERT INTO Orders (...)        │
│  └── INSERT INTO OutboxMessages (...) │ ← 同一事务
│  COMMIT                               │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│  Outbox Processor (Background Worker)│
│  ├── SELECT * FROM OutboxMessages    │
│  │   WHERE Status = 'Pending'        │
│  ├── Publish to MQ                   │
│  └── UPDATE Status = 'Published'     │
└──────────────────────────────────────┘
```

#### 消费端幂等

```
1. 收到消息 → 提取 MessageId
2. 查询幂等表 (IdempotencyKeys) 是否存在
3. 不存在 → 处理业务 → 插入幂等记录 → ACK
4. 已存在 → 直接 ACK（重复消费）
```

---

### 4.8 Ashy.Saga — 分布式事务

**定位**：提供编排式 Saga 分布式事务引擎，通过补偿操作保证最终一致性。

#### 核心抽象

```csharp
public class SagaDefinition<TData> where TData : class
{
    public string Name { get; init; }
    public List<SagaStep<TData>> Steps { get; init; } = new();
    public SagaRetryPolicy RetryPolicy { get; init; }
    public TimeSpan Timeout { get; init; }
}

public class SagaStep<TData>
{
    public string Name { get; init; }
    public Func<TData, Task> Action { get; init; }
    public Func<TData, Task>? CompensateAction { get; init; }
}

public interface ISagaManager
{
    Task<Guid> StartAsync<TData>(string sagaName, TData data);
    Task<SagaStatus> GetStatusAsync(Guid sagaId);
}

public enum SagaStatus
{
    Pending, Running, Completed, Compensating, Compensated, Failed
}
```

#### 编排式 Saga 示例

```csharp
var createOrderSaga = new SagaDefinition<OrderData>("CreateOrder")
{
    Steps = new()
    {
        new SagaStep<OrderData>
        {
            Name = "CreateOrder",
            Action = async data => await orderService.CreateAsync(data),
            CompensateAction = async data => await orderService.CancelAsync(data.OrderId)
        },
        new SagaStep<OrderData>
        {
            Name = "ReserveInventory",
            Action = async data => await inventoryService.ReserveAsync(data.Items),
            CompensateAction = async data => await inventoryService.ReleaseAsync(data.Items)
        },
        new SagaStep<OrderData>
        {
            Name = "ProcessPayment",
            Action = async data => await paymentService.ProcessAsync(data.Amount),
            CompensateAction = async data => await paymentService.RefundAsync(data.TransactionId)
        }
    },
    RetryPolicy = SagaRetryPolicy.ExponentialBackoff(maxRetries: 3),
    Timeout = TimeSpan.FromMinutes(5)
};

// 注册 Saga
builder.Services.AddAshySaga(options =>
{
    options.RegisterSaga(createOrderSaga);
    options.Persistence.UseEntityFrameworkCore();
});

// 启动 Saga
var sagaId = await sagaManager.StartAsync("CreateOrder", orderData);
```

#### Saga 执行流程

```
Start → Step1: CreateOrder → Step2: ReserveInventory → Step3: ProcessPayment → Complete
                                                                         ↓ (失败)
                     Compensate3: Refund ← Compensate2: Release ← Compensated
```

#### 持久化

- EF Core 实现：`SagaInstance` 表存储 Saga 状态、当前步骤、重试次数
- 支持跨服务实例恢复（Saga 状态不绑定到特定进程）

---

### 4.9 Ashy.Caching — 缓存

**定位**：提供统一的缓存抽象，支持 Memory 和 Redis 实现。

#### 功能清单

| 功能 | 说明 |
|------|------|
| Cache-Aside | 封装"先查缓存→未命中→查数据库→回写缓存"模式 |
| 批量失效 | 按 Tag 批量删除相关缓存项 |
| 防缓存雪崩 | 随机过期时间（±10%~30%）、互斥锁防止缓存击穿 |
| 二级缓存 | L1 Memory + L2 Redis，降低网络开销 |
| 分布式锁 | 基于 Redis 的分布式锁，用于缓存重建互斥 |

#### API 设计

```csharp
public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, CacheOptions options);
    Task RemoveAsync(string key);
    Task RemoveByTagAsync(string tag);
    Task<bool> ExistsAsync(string key);
}

public record CacheOptions
{
    public TimeSpan? AbsoluteExpiration { get; init; }
    public TimeSpan? SlidingExpiration { get; init; }
    public string[]? Tags { get; init; }
}

// Cache-Aside 扩展
public static async Task<T> GetOrSetAsync<T>(
    this ICacheProvider cache,
    string key,
    Func<Task<T>> factory,
    CacheOptions options)
{
    var cached = await cache.GetAsync<T>(key);
    if (cached is not null) return cached;

    var value = await factory();
    await cache.SetAsync(key, value, options);
    return value;
}
```

---

### 4.10 Ashy.MultiTenancy — 多租户

**定位**：提供多租户支持，包括租户解析、上下文传播和数据隔离。

#### 租户解析策略

```csharp
public enum TenantResolutionStrategy
{
    FromHeader,       // X-Tenant-Id
    FromHost,         // tenant1.example.com
    FromPath,         // /api/{tenantId}/...
    FromJwtClaim,     // JWT: tenant_id
    FromQueryString   // ?tenantId=xxx
}
```

#### API 设计

```csharp
// 注册多租户
builder.Services.AddAshyMultiTenancy(options =>
{
    options.ResolutionStrategy = TenantResolutionStrategy.FromHeader;
    options.HeaderName = "X-Tenant-Id";
    options.DataIsolation = DataIsolationStrategy.DatabasePerTenant;
});

// 获取当前租户
public interface ITenantContext
{
    string? TenantId { get; }
    TenantInfo? Tenant { get; }
}

// EF Core 全局查询过滤器
modelBuilder.Entity<Order>()
    .HasQueryFilter(o => o.TenantId == tenantContext.TenantId);

// 或自动应用（共享表方案）
builder.Services.AddAshyMultiTenancy(options =>
{
    options.DataIsolation = DataIsolationStrategy.SharedTableWithFilter;
    options.AutoApplyGlobalQueryFilter = true;
});
```

#### 数据隔离策略对比

| 策略 | 隔离级别 | 性能 | 复杂度 | 适用场景 |
|------|----------|------|--------|----------|
| Database-per-Tenant | 最强 | 中 | 低 | 高安全性要求 |
| Schema-per-Tenant | 强 | 中 | 中 | PostgreSQL / SQL Server |
| Shared Table + Filter | 弱 | 高 | 低 | SaaS 小租户 |

#### 租户上下文跨服务传播

- HTTP: `X-Tenant-Id` Header 自动传递
- gRPC: `ashy-tenant-id` Metadata 自动传递
- EventBus: 事件消息自动携带 `TenantId`

---

### 4.11 Ashy.BackgroundJobs — 后台任务

**定位**：基于 Quartz.NET 提供分布式定时任务调度。

#### 功能清单

| 功能 | 说明 |
|------|------|
| Cron 调度 | 支持 Cron 表达式 |
| 持久化 | Job 定义和触发记录持久化到数据库 |
| 分布式锁 | 基于 Redis/DB 的分布式锁，确保单节点执行 |
| 管理 API | 查看/触发/暂停/恢复任务 |
| HTTP 回调 | 支持 HTTP/gRPC 回调式 Job |

#### API 设计

```csharp
// 注册
builder.Services.AddAshyBackgroundJobs(options =>
{
    options.UseQuartz();
    options.UseEntityFrameworkPersistence();
});

// 定义 Job
public class DailyReportJob : IAshyJob
{
    public Task ExecuteAsync(JobExecutionContext context)
    {
        // 生成日报
        return Task.CompletedTask;
    }
}

// 注册调度
builder.Services.AddJob<DailyReportJob>(config =>
{
    config.WithCronSchedule("0 0 8 * * ?"); // 每天早上 8:00
    config.WithDescription("生成每日销售报表");
});

// 管理 API
GET  /ashy/jobs          # 列出所有 Job
POST /ashy/jobs/{name}/trigger  # 手动触发
POST /ashy/jobs/{name}/pause    # 暂停
POST /ashy/jobs/{name}/resume   # 恢复
```

---

### 4.12 Ashy.DDD — 领域驱动设计构建块

**定位**：提供 DDD 战术设计的基类和工具。

#### 构建块清单

| 构建块 | 说明 |
|--------|------|
| `Entity<TKey>` | 实体基类，包含 Id 和相等性比较 |
| `AggregateRoot<TKey>` | 聚合根基类，管理领域事件集合 |
| `ValueObject` | 值对象基类，值相等性比较 |
| `IDomainEvent` | 领域事件接口 |
| `Enumeration` | 枚举类模式基类 |
| `ISpecification<T>` | 规约模式接口 |
| `IRepository<T>` | 通用仓储接口 |
| `IUnitOfWork` | 工作单元接口 |

#### 核心抽象

```csharp
public abstract class AggregateRoot<TKey> : Entity<TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}

public interface IRepository<T> where T : AggregateRoot<Guid>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
    Expression<Func<T, bool>> ToExpression();
}

// 领域事件分发
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
}
```

---

### 4.13 Ashy.Dapr — Dapr 集成（可选）

**定位**：作为可选的集成层，将 Dapr 的分布式能力适配到 Ashy 接口体系。

#### 适配映射

| Dapr Building Block | Ashy 接口 |
|---------------------|-----------|
| Pub/Sub | `IEventBus` |
| State Store | `ICacheProvider` |
| Secret Store | `IConfiguration` |
| Service Invocation | `IAshyHttpClient` |
| Actor | `DaprActor<T>` 封装 |
| Distributed Lock | 分布式锁抽象 |

---

## 5. 技术栈选型

| 类别 | 技术 | 版本 | 说明 |
|------|------|------|------|
| 运行时 | .NET | 10.0 | 最新 LTS |
| Web 框架 | ASP.NET Core Minimal API | 10.0 | 轻量高性能 |
| 反向代理 | YARP | 2.x | 微软官方高性能代理 |
| 弹性策略 | Polly | 8.x | 断路器、重试、超时 |
| gRPC | Grpc.AspNetCore | 2.x | 原生集成 |
| 可观测性 | OpenTelemetry | 1.x | 官方 SDK |
| 日志 | Serilog | 4.x | 结构化日志 |
| ORM | Entity Framework Core | 10.0 | 数据访问 |
| 缓存 | StackExchange.Redis | 2.x | Redis 客户端 |
| 注册中心 | Nacos SDK for .NET | 2.x | 服务发现+配置 |
| 定时任务 | Quartz.NET | 3.x | 任务调度 |
| MQ | RabbitMQ.Client | 7.x | RabbitMQ |
| MQ | Confluent.Kafka | 2.x | Kafka |
| 测试框架 | xUnit | 2.x | 单元测试 |
| 集成测试 | Testcontainers | 4.x | Docker 集成测试 |
| 性能测试 | BenchmarkDotNet | 0.x | 基准测试 |
| 文档站点 | Docusaurus / Statiq | - | 静态文档生成 |
| CI/CD | GitHub Actions | - | 自动构建发布 |

---

## 6. 分阶段路线图

### P0: 基础建设 (Month 1-2)

| 任务 | 详情 |
|------|------|
| 创建 Solution + 项目骨架 | `Ashy.sln`, `Directory.Packages.props`, `Directory.Build.props` |
| Ashy.Core | `IAshyHostBuilder`, `IModule`, `ICorrelationContext`, `ApiResult<T>`, 异常处理 |
| CI/CD 搭建 | GitHub Actions：build → test → pack → publish NuGet |
| 文档站点 | Docusaurus 脚手架 + GitHub Pages 部署 |
| 开发规范 | `.editorconfig`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md` |
| 基础示例 | `BasicMicroservice` 最小示例 |

**里程碑**: NuGet 上架 `Ashy.Core` v0.1.0-preview

---

### P1: 服务通信 (Month 3-4)

| 任务 | 详情 |
|------|------|
| Ashy.Nacos | 服务注册、发现、心跳、优雅下线 |
| Ashy.Nacos | 配置中心集成、热刷新、多 DataId |
| Ashy.ServiceProxy | HTTP 客户端代理、负载均衡、服务发现解析 |
| Ashy.ServiceProxy | gRPC 客户端代理、自定义 Resolver |
| Ashy.ServiceProxy | Polly 弹性管道（重试/断路器/超时/舱壁） |
| Health Checks | Liveness / Readiness / Startup 探针 |

**里程碑**: `Ashy.Nacos` + `Ashy.ServiceProxy` v0.2.0-preview

---

### P2: 网关与配置 (Month 5-6)

| 任务 | 详情 |
|------|------|
| Ashy.Gateway | YARP 集成、动态路由提供程序 |
| Ashy.Gateway | 认证中继、限流中间件 |
| Ashy.Gateway | gRPC-Web 转码 |
| Ashy.Gateway | 管理 API（运行时路由更新） |
| Ashy.Nacos | 共享配置、配置监听回调 |

**里程碑**: `Ashy.Gateway` v0.3.0-preview

---

### P3: 可观测性 (Month 7-8)

| 任务 | 详情 |
|------|------|
| Ashy.Observability | OpenTelemetry Tracing 自动装配 |
| Ashy.Observability | Prometheus Metrics + Grafana 仪表盘 |
| Ashy.Observability | Serilog 集成，结构化日志 |
| Ashy.Observability | Health Check UI |
| 上下文传播 | 全模块 TraceId/SpanId/TenantId 自动传播 |

**里程碑**: `Ashy.Observability` v0.4.0-preview

---

### P4: 消息与事务 (Month 9-10)

| 任务 | 详情 |
|------|------|
| Ashy.Messaging | IEventBus 抽象 + InMemory 实现 |
| Ashy.Messaging.RabbitMQ | 交换机/队列声明、DLQ、延迟消息 |
| Ashy.Messaging.Kafka | 分区、消费者组、Schema Registry |
| Ashy.Messaging | 事务性发件箱 (Outbox Pattern) |
| Ashy.Messaging | 消费端幂等 |
| Ashy.Saga | 编排式 Saga 引擎、状态持久化 |

**里程碑**: `Ashy.Messaging` + `Ashy.Saga` v0.5.0-preview

---

### P5: 企业级能力 (Month 11-12)

| 任务 | 详情 |
|------|------|
| Ashy.Auth.Jwt | JWT 验证、令牌中继、动态密钥刷新 |
| Ashy.Caching.Redis | Cache-Aside、二级缓存、防雪崩 |
| Ashy.MultiTenancy | 租户解析、上下文传播、数据隔离 |
| Ashy.BackgroundJobs | Quartz.NET 集成、管理 API |

**里程碑**: v0.6.0-preview

---

### P6: DDD & Dapr (Month 13-14)

| 任务 | 详情 |
|------|------|
| Ashy.DDD | Entity, AggregateRoot, ValueObject, DomainEvent |
| Ashy.DDD | Specification, Repository, UnitOfWork |
| Ashy.Dapr | 适配 IEventBus / ICacheProvider / ServiceProxy |
| 完整示例 | `EShopOnAshy` 电商微服务示例 |

**里程碑**: v0.7.0-preview

---

### P7: 打磨与发布 (Month 15-16)

| 任务 | 详情 |
|------|------|
| 性能基准测试 | BenchmarkDotNet 各模块性能报告 |
| 压测 | K6/Gatling 负载测试 |
| 安全审计 | 依赖扫描、漏洞修复 |
| 文档完善 | API 文档、架构决策记录、最佳实践 |
| 社区建设 | 博客推广、.NET Conf 演讲、贡献者指南 |
| Release | **v1.0.0 正式版发布** |

---

## 7. 开源社区运营策略

| 维度 | 策略 |
|------|------|
| **开源协议** | MIT |
| **代码仓库** | GitHub Monorepo，统一管理 |
| **包发布** | NuGet.org，模块独立版本号 |
| **版本策略** | Semantic Versioning 2.0 |
| **CI/CD** | GitHub Actions：PR 自动 build+test，合并 main 自动发布 NuGet |
| **Pull Request** | 模板 + 代码审查 + 所有检查通过 + 文档更新要求 |
| **Issues** | Bug Report / Feature Request / Question 模板 |
| **文档** | Docusaurus 站点 + 各模块 README + ADR 存档 |
| **治理模型** | Benevolent Dictator → Maintainer Team（3 阶段：个人→核心团队→社区治理） |
| **社区渠道** | GitHub Discussions / Discord / 知乎专栏同步 |
| **推广** | .NET Conf 演讲 / Reddit r/dotnet / .NET 微信公众号 |
| **署名** | 所有贡献者列入 CONTRIBUTORS.md |
| **示例驱动** | 优先发布高质量示例（eShopOnAshy），用实际场景证明价值 |

---

## 8. 关键风险与缓解

| 风险 | 严重度 | 缓解措施 |
|------|--------|----------|
| 个人精力有限，范围过大 | 🔴 高 | 严格按模块 MVP 优先，每个 Phase 只发布 2-3 个核心包；非核心模块（DDD/Dapr/多租户）可延后 |
| Nacos .NET SDK 不稳定 | 🟡 中 | 抽象一层 INacosClient 接口，必要时可替换 SDK 实现 |
| .NET 社区已有 Steeltoe/ABP 等竞品 | 🟡 中 | 差异化：专注 .NET 10 新特性、Minimal API、Nacos 生态、gRPC 双协议、自带 Grafana 仪表盘 |
| 文档落后于代码 | 🟡 中 | CI 检查每个 PR 是否包含 XML Doc 注释和文档更新 |
| 社区冷启动无人关注 | 🟡 中 | 从 eShopOnAshy 完整示例切入，解决开发者实际痛点；在 .NET Conf 等会议做分享 |
| .NET 10 API 变更 | 🟢 低 | 微软的 API 兼容性承诺；即便是预览版，变动范围可控 |
| Dapr 自身迭代快 | 🟢 低 | Ashy.Dapr 是可选模块，不影响核心功能；按需跟进适配 |

---

## 附录 A：快速入门示例（预期效果）

```csharp
// Program.cs — 一个完整的微服务入口
var builder = WebApplication.CreateBuilder(args);

builder.AddAshy(new AshyOptions
{
    ServiceName = "order-service",
    Environment = "production"
})
.UseNacos(nacos =>
{
    nacos.ServerAddresses = new[] { "http://nacos:8848" };
})
.UseServiceProxy()
.UseObservability()
.UseMessaging(mq => mq.UseRabbitMQ())
.UseCaching(cache => cache.UseRedis("redis:6379"))
.UseMultiTenancy()
.Build();

// 业务端点
app.MapGet("/api/orders/{id}", async (Guid id, IOrderRepository repo) =>
{
    var order = await repo.GetByIdAsync(id);
    return Results.Ok(new ApiResult<Order>(true, null, order));
});

app.Run();
```

---

> 📄 本文档为 Ashy 框架的总体设计计划，版本 v1.0，最后更新 2026-05-16。
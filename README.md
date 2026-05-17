# Ashy

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=.net" alt=".NET 10.0" />
  <img src="https://img.shields.io/badge/NuGet-1.0.0-004880?style=flat-square&logo=nuget" alt="NuGet 1.0.0" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg?style=flat-square" alt="MIT License" />
  <img src="https://img.shields.io/badge/stage-early--stage-orange?style=flat-square" alt="Early Stage" />
</p>

<p align="center">
  <strong>面向 .NET 10 的企业级基础设施库 —— 标准化、可组合、可观测</strong>
</p>

---

## 介绍

Ashy 是一套面向 .NET 10 的**企业级基础设施库**，为后端系统开发中的通用能力提供标准化实现。涵盖序列化、加密、配置、HTTP 客户端、对象存储、消息队列、服务发现、健康检查、可观测性等常见技术组件，由独立 NuGet 包组成，按需引用。

在设计上，Ashy 追求以下目标：

- **可组合**：每个包职责单一、边界清晰，不传递无关依赖，可按场景自由装配
- **一致性**：统一的命名约定、注册模式与错误模型，降低团队在不同模块间的切换成本
- **可观测**：集成 OpenTelemetry、Prometheus、SkyWalking 等主流观测方案，适配企业级运维要求
- **渐进式**：核心能力轻量无侵入，高级功能通过独立包按需引入

## 架构概览

```
┌──────────────────────────────────────────────────────────────┐
│                      API Gateway                             │
│               (YARP + Dynamic Routing)                       │
├──────────────────────────────────────────────────────────────┤
│   Auth (JWT/OAuth2)  │  Rate Limiting  │  Transformation     │
├──────────────────────────────────────────────────────────────┤
│                Service Communication Layer                   │
│       HTTP(REST) <> gRPC  │  Service Discovery  │  LB        │
├──────────────────────────────────────────────────────────────┤
│   Resilience Pipeline                                        │
│   Circuit Breaker │ Retry │ Timeout │ Bulkhead               │
├──────────────────────────────────────────────────────────────┤
│                    Service Layer                             │
│                                                              │
│   ┌─────────────────────────┐   ┌─────────────────────────┐  │
│   │   Business Services     │   │ Infrastructure Services │  │
│   │                         │   │                         │  │
│   │   - OrderService        │   │ Nacos (Register/Config) │  │
│   │   - PaymentService      │   │ Caching (Redis)         │  │
│   │   - InventoryService    │   │ Event Bus (MQ)          │  │
│   │   - UserService         │   │ Saga Manager            │  │
│   │                         │   │ Background Jobs         │  │
│   └─────────────────────────┘   └─────────────────────────┘  │
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
  │   ├── ServiceProxy → 调用下游服务
  │   ├── Messaging → 发布领域事件
  │   ├── Caching → 缓存读写
  │   └── Saga → 执行分布式事务
  │
  └── 响应返回
      └── 统一响应模型 (ApiResult<T>)
```

## 快速开始

### 安装

核心包已发布到 NuGet，其余包随开发进度陆续上架。

```bash
# 核心库
dotnet add package Ashy

# ASP.NET Core 集成
dotnet add package Ashy.AspNetCore
```

### 注册服务

```csharp
using Ashy.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 一行接入 Ashy
builder.Services.AddAshy();

var app = builder.Build();
app.UseAshy();
app.Run();
```

### 序列化

```csharp
using Ashy.Serialization;

var user = new User { Name = "张三" };

// XML
var xml = user.ToXml();
var fromXml = xml.FromXml<User>();

// JSON
var json = user.ToJson();
var fromJson = json.FromJson<User>();

// 异步版本
var xmlAsync = await user.ToXmlAsync();
var jsonAsync = await user.ToJsonAsync();
```

### 完整示例

以下展示 Ashy 目标形态的链式构建器：

```csharp
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

app.MapGet("/api/orders/{id}", async (Guid id, IOrderRepository repo) =>
{
    var order = await repo.GetByIdAsync(id);
    return Results.Ok(new ApiResult<Order>(true, null, order));
});

app.Run();
```

## 组件地图

### 核心

| 状态 | 包名 | 说明 |
| :---: | ---- | ---- |
| ✅ | **Ashy** | 核心库：通用扩展、XML/JSON 序列化 |
| ✅ | **Ashy.AspNetCore** | ASP.NET Core 集成：服务注册、中间件、DI 增强、API 版本管理 |
| 🚧 | Ashy.Cryptography | 对称 / 非对称加密、哈希、签名、证书管理 |
| 🚧 | Ashy.Configuration | 多源配置合并、选项模式增强、热更新 |
| 🚧 | Ashy.Http | HttpClient 封装：重试策略、熔断、链路传递 |
| 🚧 | Ashy.Storage | 文件存储抽象层：本地、OSS、MinIO 统一接口 |
| 🚧 | Ashy.EventBus | 进程内事件总线：发布/订阅，模块间解耦通信 |
| 🚧 | Ashy.HealthCheck | 健康检查：数据库、Redis、RabbitMQ 等组件探活 |
| 🚧 | Ashy.Mapster | 对象映射，Mapster 开箱封装 |
| 🚧 | Ashy.Serilog | Serilog 集成：Enricher、Sink 一站配置 |
| 🚧 | Ashy.QRCode | 二维码生成与解析 |
| 🚧 | Ashy.Email | 邮件投递：SMTP、主流邮件服务适配 |
| 🚧 | Ashy.Office | 文档处理：Excel 导入导出、Word 模板填充 |
| 🚧 | Ashy.Network | 网络工具：文件上传下载、FTP 客户端 |
| 🚧 | Ashy.Monitor | 系统监控：宿主机硬件指标、进程探活 |
| 🚧 | Ashy.Auth | 认证与授权：JWT 签发/校验、OAuth2/OIDC 集成、策略式授权 |
| 🚧 | Ashy.Audit | 审计日志：实体变更追踪、操作留痕、审计日志持久化 |
| 🚧 | Ashy.Compression | 响应压缩：Gzip/Brotli 中间件，请求/响应自动加解压 |
| 🚧 | Ashy.Csv | CSV 解析与生成，与 Office 互补 |
| 🚧 | Ashy.DataProtection | 敏感数据脱敏、加解密标记注解 |
| 🚧 | Ashy.FeatureFlags | 特性开关：灰度发布、A/B 测试、按租户/用户分流 |
| 🚧 | Ashy.ImageProcessor | 图片处理：缩放、裁剪、水印、格式转换 |
| 🚧 | Ashy.Localization | 国际化：资源文件管理、多语言中间件、数据库驱动翻译 |
| 🚧 | Ashy.MultiTenancy | 多租户：租户解析策略（Header/Host/Path）、租户级 DI、数据隔离 |
| 🚧 | Ashy.RateLimiting | 限流：固定窗口/滑动窗口/令牌桶，ASP.NET Core 中间件集成 |
| 🚧 | Ashy.Resilience | Polly 策略封装：重试/熔断/超时/舱壁，与 Ashy.Http 协同 |
| 🚧 | Ashy.Validation | 请求校验：FluentValidation 开箱集成、自定义规则库 |
| 🚧 | Ashy.BusinessRule | 业务规则引擎：规则定义、条件组合、优先级排序 |
| 🚧 | Ashy.Captcha | 验证码：reCAPTCHA/hCaptcha/滑块验证，开箱即用 |
| 🚧 | Ashy.CQRS | 命令查询职责分离：MediatR 集成、Pipeline Behavior |
| 🚧 | Ashy.DataPermission | 数据权限：行级过滤、列级脱敏、动态 Scope 注入 |
| 🚧 | Ashy.DynamicApi | 动态 API：从 Service 接口自动生成 HTTP 端点（对标 ABP Auto API） |
| 🚧 | Ashy.EventSourcing | 事件溯源：事件存储、快照、投影重放 |
| 🚧 | Ashy.IdGeneration | 分布式 ID：Snowflake / UUID v7 / 号段模式 |
| 🚧 | Ashy.Notification | 统一通知中心：邮件 + 短信 + SignalR + 站内信，模板化推送 |
| 🚧 | Ashy.Setting | 设置管理：多租户感知、加密存储、热更新（对标 ABP Settings） |
| 🚧 | Ashy.SMS | 短信发送：阿里云/腾讯云/Twilio 适配，渠道切换 |
| 🚧 | Ashy.Template | 模板引擎：Razor / Scriban / Liquid 渲染，邮件/报表模板 |
| 🚧 | Ashy.VirtualFileSystem | 虚拟文件系统：嵌入式资源、模块化文件管理（对标 ABP VFS） |

### 基础设施

| 状态 | 包名 | 说明 |
| :---: | ---- | ---- |
| 🚧 | Ashy.Redis | Redis 客户端：字符串/哈希/列表等原生操作、分布式锁、发布订阅 |
| 🚧 | Ashy.MongoDB | MongoDB 驱动封装，Repository 模式 |
| 🚧 | Ashy.RabbitMQ | RabbitMQ 消息队列：发布 / 订阅、RPC |
| 🚧 | Ashy.Kafka | Kafka 客户端：生产者/消费者、死信处理、重试 |
| 🚧 | Ashy.ElasticSearch | Elasticsearch 客户端：日志检索、聚合分析 |
| 🚧 | Ashy.Consul | 服务注册发现、健康检查上报、KV 配置中心 |
| 🚧 | Ashy.Nacos | 服务注册发现、配置管理（Nacos 适配） |
| 🚧 | Ashy.Grpc | gRPC 客户端工厂、拦截器、ProtoBuf 工具 |
| 🚧 | Ashy.Prometheus | 指标采集：Counter / Gauge / Histogram |
| 🚧 | Ashy.SignalR | SignalR Hub 管理、Redis 背板、连接追踪 |
| 🚧 | Ashy.Skywalking | SkyWalking 链路追踪集成 |
| 🚧 | Ashy.OpenTelemetry | 可观测性三件套：Trace / Metrics / Logging 一站式导出 |
| 🚧 | Ashy.Hangfire | Hangfire 任务调度封装 |
| 🚧 | Ashy.Swagger | Swagger / OpenAPI 文档生成 |
| 🚧 | Ashy.Scalar | Scalar API 文档集成（现代化 Swagger UI 替代） |
| 🚧 | Ashy.Workflow | 轻量级工作流引擎 |
| 🚧 | Ashy.MiniProfiler | 性能监控与 SQL 分析 |
| 🚧 | Ashy.WebRTC | WebRTC 实时通信 |
| 🚧 | Ashy.WebSocket | WebSocket 连接管理与消息推送 |
| 🚧 | Ashy.Caching | 缓存抽象：ICacheProvider、Cache-Aside、L1 Memory + L2 可插拔后端 |
| 🚧 | Ashy.EFCore | EF Core 封装：Interceptor、Repository/UoW 模式、审计追踪 |
| 🚧 | Ashy.Quartz | 分布式定时任务：Quartz.NET 封装、持久化、集群调度 |
| 🚧 | Ashy.ClickHouse | ClickHouse 客户端：OLAP 分析查询、批量写入、物化视图 |
| 🚧 | Ashy.FreeSql | FreeSql ORM 适配：与 Ashy 仓储/工作单元模式对齐 |
| 🚧 | Ashy.GraphQL | GraphQL 端点：HotChocolate 集成，查询/变更/订阅 |
| 🚧 | Ashy.DistributedLock | 分布式锁：Redis / ZooKeeper / etcd 统一抽象 |
| 🚧 | Ashy.Payment | 支付抽象：支付宝/微信/Stripe，统一的支付/退款/回调 |
| 🚧 | Ashy.Search | 搜索抽象：Lucene + Elasticsearch，索引管理、高亮、聚合 |
| 🚧 | Ashy.Sharding | 数据库分片：读写分离、分库分表路由、自动迁移 |

> ✅ 已可用&emsp;🚧 开发中

## 场景推荐

| 场景 | 选配建议 |
| ---- | -------- |
| 单体应用 | Ashy + Ashy.AspNetCore + Ashy.Auth + Ashy.EFCore + Ashy.Validation + Ashy.Swagger + Ashy.CQRS + Ashy.DynamicApi + Ashy.Setting |
| 微服务集群 | + Ashy.Consul + Ashy.Grpc + Ashy.RabbitMQ + Ashy.Redis + Ashy.Skywalking + Ashy.Resilience + Ashy.RateLimiting + Ashy.GraphQL + Ashy.DistributedLock + Ashy.Search + Ashy.Notification |
| 多租户 SaaS | + Ashy.MultiTenancy + Ashy.MongoDB + Ashy.ElasticSearch + Ashy.Workflow + Ashy.Prometheus + Ashy.Audit + Ashy.FeatureFlags + Ashy.DataPermission + Ashy.Sharding + Ashy.EventSourcing + Ashy.BusinessRule + Ashy.Payment |

## 竞品对比

| 对比维度 | Ashy | Steeltoe | ABP Framework |
|----------|------|----------|---------------|
| .NET 版本 | .NET 10 原生 | 适配多版本 | 适配多版本 |
| 注册中心 | Nacos 优先 | Consul/Eureka | 无内置 |
| 配置中心 | Nacos 一体化 | Spring Cloud Config | 自研 |
| API 风格 | Minimal API 优先 | Controller | Controller |
| 网关 | YARP 动态路由 | 无内置 | 无内置 |
| Native AOT | 目标支持 | 部分支持 | 不支持 |
| 协议 | HTTP REST + gRPC 双协议 | HTTP | HTTP |
| CQRS | 内建 | 无 | 无 |
| Dynamic API | 内建 | 无 | 内建 |
| GraphQL | 内建 | 无 | 需额外模块 |
| 支付 | 内建 | 无 | 需商业版 |
| 分布式锁 | 内建 | 无 | 无 |
| 数据库分片 | 内建 | 无 | 无 |
| 事件溯源 | 内建 | 无 | 无 |

## 技术栈

| 类别 | 技术 | 说明 |
|------|------|------|
| 运行时 | .NET 10 | 最新 LTS |
| Web 框架 | ASP.NET Core Minimal API | 轻量高性能 |
| 反向代理 | YARP 2.x | 微软官方高性能代理 |
| 弹性策略 | Polly 8.x | 断路器、重试、超时 |
| 可观测性 | OpenTelemetry 1.x | Trace / Metrics / Logging |
| 日志 | Serilog 4.x | 结构化日志 |
| ORM | EF Core 10.0 | 数据访问 |
| 缓存 | StackExchange.Redis 2.x | Redis 客户端 |
| 注册中心 | Nacos SDK 2.x | 服务发现 + 配置 |
| 定时任务 | Quartz.NET 3.x | 任务调度 |
| 消息队列 | RabbitMQ.Client 7.x / Confluent.Kafka 2.x | 双 MQ 支持 |
| 测试 | xUnit 2.x + Testcontainers 4.x | 单元测试 + 集成测试 |
| 性能测试 | BenchmarkDotNet 0.x | 基准测试 |

## 开发要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- 任意 IDE / 编辑器（VS 2026+、Rider、VS Code）

```bash
git clone https://github.com/GrayJunZi/Ashy.git
cd Ashy
dotnet build
dotnet test
```

## 路线图

| 阶段 | 周期 | 核心交付 |
|------|------|----------|
| **P0 基础建设** | Month 1-2 | Ashy.Core（Host 构建器、模块系统、上下文传播、统一响应）、CI/CD、文档站点 |
| **P1 服务通信** | Month 3-4 | Ashy.Nacos（注册/发现/配置）、Ashy.ServiceProxy（HTTP + gRPC 代理、负载均衡、弹性管道） |
| **P2 网关** | Month 5-6 | Ashy.Gateway（YARP 动态路由、认证中继、限流、gRPC-Web 转码） |
| **P3 可观测性** | Month 7-8 | Ashy.Observability（OpenTelemetry 全链路追踪、Prometheus 指标、Serilog 集成、Health Check UI） |
| **P4 消息与事务** | Month 9-10 | Ashy.Messaging（RabbitMQ + Kafka、Outbox 模式、消费端幂等）、Ashy.Saga（编排式 Saga 引擎） |
| **P5 企业级能力** | Month 11-12 | Ashy.Auth（JWT + OAuth2）、Ashy.Caching（二级缓存）、Ashy.MultiTenancy、Ashy.BackgroundJobs |
| **P6 DDD & Dapr** | Month 13-14 | Ashy.DDD（Entity/AggregateRoot/Repository/UoW）、Ashy.Dapr 适配层、eShopOnAshy 完整示例 |
| **P7 打磨发布** | Month 15-16 | 性能基准、安全审计、文档完善、v1.0.0 正式版发布 |

## 贡献

本项目尚在早期开发阶段，欢迎 Issue 和 PR。

1. Fork 本仓库
2. 从 `main` 创建特性分支
3. 提交代码并附上清晰的 commit message
4. 发起 PR，描述做了什么以及为什么

## 协议

[MIT](LICENSE) · Copyright © 2026 GrayJunZi
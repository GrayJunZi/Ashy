# AGENTS.md

## 项目概述

**Ashy** — .NET 10 企业级基础设施库。独立 NuGet 包组成的微服务全家桶。

## 技术栈

| 项 | 选型 |
|---|------|
| 运行时 | .NET 10（`net10.0`） |
| Web 框架 | ASP.NET Core Minimal API |
| 测试 | xUnit v3 + Testcontainers |
| 性能 | BenchmarkDotNet |
| 序列化 | System.Text.Json |

## 构建命令

```powershell
dotnet build
dotnet test
dotnet pack
```

## 架构约定

### 项目分层

```
src/Ashy/                  # 核心库 — 零外部依赖，纯 BCL
src/Ashy.AspNetCore/       # ASP.NET Core 集成 — FrameworkReference
src/Ashy.XXX/              # 功能模块包 — 各自独立
tests/Ashy.UnitTests/      # 单元测试
```

### 目标框架

所有项目 `TargetFramework` = `net10.0`，`Nullable` = `enable`，`ImplicitUsings` = `enable`。

### 包引用规则

- `src/Ashy/` 禁止引入任何外部 NuGet 包
- 功能模块按需引入依赖，不传递无关依赖
- 使用 `Directory.Packages.props` 集中管理版本（如引入新包需先添加）

### 项目引用规则

每个功能模块可引用 `Ashy`，但不应引用其他功能模块（保持解耦）。

## 代码惯例

### namespace 即文件夹

```
src/Ashy/Context/CorrelationContext.cs  →  namespace Ashy.Context;
src/Ashy/Modules/IModule.cs             →  namespace Ashy.Modules;
```

### 类型命名

- 接口：`I` 前缀（`IModule`, `IExceptionMapper`）
- 基类：`Base` 后缀或直接用抽象类
- 扩展方法：静态类名 = `<被扩展类型>Extensions`
- Options 类：`<功能名>Options`，使用 `[ValidateDataAnnotations]`

### 访问修饰符

- 所有需要测试的类型为 `public`
- 内部实现细节用 `internal` + `InternalsVisibleTo`（如需要）

### DI 注册

扩展方法放在 `Extensions/` 文件夹下：
- `IServiceCollection` 扩展：`ServiceCollectionExtensions.cs`
- `IApplicationBuilder` 扩展：`ApplicationBuilderExtensions.cs`

### 记录类型

- 统一使用 `record`（不混用 `class`），数据模型使用 positional record，DTO 使用 nominal record

## 工作流：Spec-Driven Development

本项目采用 **spec-first** 开发模式。实现任何功能前，必须先有对应的 spec 文件。

### 工作流程

```
写 Spec → Review Spec → AI/开发者按 Spec 实现 → 跑测试 → Review 代码
```

### Spec 文件位置

```
docs/specs/
├── 01-core.md              # Ashy.Core 实现 spec
├── 02-nacos.md             # Ashy.Nacos
└── ...
```

### Spec 文件应包含

1. **目标**：要实现什么
2. **接口定义**：精确的 C# 签名（interface/record/class 骨架）
3. **DI 注册模式**：扩展方法签名
4. **错误处理**：抛什么异常，如何映射
5. **测试用例骨架**：关键测试场景
6. **验收标准**：怎么算"做完"

AI 工具在实现时，应：
1. 先读对应的 spec 文件
2. 严格按照 spec 中的接口签名实现
3. 不自行添加 spec 外的功能和接口
4. 完成后跑 `dotnet build` 和 `dotnet test`

## 添加新模块

1. 在 `docs/specs/` 下创建模块 spec
2. 创建项目文件夹 `src/Ashy.XXX/`
3. 添加到 `Ashy.slnx`
4. 创建测试项目 `tests/Ashy.XXX.Tests/`
5. 更新 `Directory.Packages.props`（如有新依赖）
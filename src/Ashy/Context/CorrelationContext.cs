using System.Diagnostics;

namespace Ashy.Context;

/// <summary>
/// 跨服务上下文，基于 W3C TraceContext 标准。
/// 通过 ICorrelationContextAccessor 在异步调用链中自动传播。
/// </summary>
public class CorrelationContext
{
    /// <summary>
    /// W3C 追踪 ID（32 位十六进制）
    /// </summary>
    public string TraceId { get; init; }

    /// <summary>
    /// W3C 跨度 ID（16 位十六进制）
    /// </summary>
    public string SpanId { get; init; }

    /// <summary>
    /// W3C 追踪标志（01 = 采样）
    /// </summary>
    public string TraceFlags { get; init; }

    /// <summary>
    /// W3C 供应商扩展状态
    /// </summary>
    public string? TraceState { get; init; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// 用户 ID
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public IDictionary<string, object> Items { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// 生成 W3C traceparent 头值（version-traceId-spanId-traceFlags）
    /// </summary>
    public string TraceParent => $"00-{TraceId}-{SpanId}-{TraceFlags}";

    /// <summary>
    /// 初始化上下文，自动继承 Activity.Current 的 TraceId / SpanId，
    /// 无 Activity 时生成符合 W3C 格式的新 ID
    /// </summary>
    public CorrelationContext()
    {
        var activity = Activity.Current;
        TraceId = activity?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        SpanId = activity?.SpanId.ToString() ?? GenerateSpanId();
        TraceFlags = activity?.ActivityTraceFlags == ActivityTraceFlags.Recorded ? "01" : "00";
    }

    /// <summary>
    /// 从 W3C traceparent 头解析上下文
    /// </summary>
    public static CorrelationContext FromTraceParent(string traceParent)
    {
        var parts = traceParent.Split('-');
        if (parts.Length != 4)
            throw new FormatException($"Invalid traceparent format: {traceParent}");

        return new CorrelationContext
        {
            TraceId = parts[1],
            SpanId = parts[2],
            TraceFlags = parts[3]
        };
    }

    /// <summary>
    /// 创建新的根上下文
    /// </summary>
    public static CorrelationContext Create(string? tenantId = null, string? userId = null)
    {
        return new CorrelationContext
        {
            TenantId = tenantId,
            UserId = userId
        };
    }

    private static string GenerateSpanId()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        return Convert.ToHexString(bytes)[..16];
    }
}
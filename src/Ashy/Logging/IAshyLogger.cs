namespace Ashy.Logging;

/// <summary>
/// Ashy 结构化日志抽象（P0 阶段接口预留，实际日志使用 ILogger&lt;T&gt;）
/// 后续 Ashy.Serilog 模块将提供扩展实现
/// </summary>
public interface IAshyLogger
{
    /// <summary>输出 Trace 级别日志</summary>
    void Trace(string message, params object?[] args);
    /// <summary>输出 Debug 级别日志</summary>
    void Debug(string message, params object?[] args);
    /// <summary>输出 Info 级别日志</summary>
    void Info(string message, params object?[] args);
    /// <summary>输出 Warn 级别日志</summary>
    void Warn(string message, params object?[] args);
    /// <summary>输出 Error 级别日志</summary>
    void Error(string message, params object?[] args);
    /// <summary>输出 Fatal 级别日志</summary>
    void Fatal(string message, params object?[] args);
}
namespace Ashy.Context;

/// <summary>
/// CorrelationContext 访问器，通过 DI 注入使用（便于单元测试 mock）
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// 当前请求的 CorrelationContext
    /// </summary>
    CorrelationContext? Current { get; set; }
}

/// <summary>
/// 基于 AsyncLocal 的默认实现
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContextHolder> _current = new();

    /// <inheritdoc />
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
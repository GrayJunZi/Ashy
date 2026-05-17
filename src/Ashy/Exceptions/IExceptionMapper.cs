namespace Ashy.Exceptions;

/// <summary>
/// 异常 → ProblemDetail 映射器
/// </summary>
public interface IExceptionMapper
{
    /// <summary>
    /// 将异常映射为 RFC 7807 问题详情
    /// </summary>
    ProblemDetail Map(Exception exception);
}
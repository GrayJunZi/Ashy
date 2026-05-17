namespace Ashy.Exceptions;

/// <summary>
/// RFC 7807 问题详情
/// </summary>
public record ProblemDetail
{
    /// <summary>
    /// 问题类型 URI
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// 简短描述
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// 详细说明
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// 出错资源的路径
    /// </summary>
    public string? Instance { get; init; }
}
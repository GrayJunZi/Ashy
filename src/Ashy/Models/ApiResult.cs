namespace Ashy.Models;

/// <summary>
/// 统一响应模型
/// </summary>
public record ApiResult<T>(bool Success, string? Message, T? Data)
{
    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResult<T> Ok(T data) => new(true, null, data);

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResult<T> Fail(string message) => new(false, message, default);
}

/// <summary>
/// 分页响应模型
/// </summary>
public record PagedResult<T>(int Page, int PageSize, long Total, IReadOnlyList<T> Items)
{
    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNextPage => Page * PageSize < Total;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevPage => Page > 1;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
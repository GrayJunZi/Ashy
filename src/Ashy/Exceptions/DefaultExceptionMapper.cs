namespace Ashy.Exceptions;

/// <summary>
/// 默认异常映射器，覆盖常见异常类型
/// </summary>
public class DefaultExceptionMapper : IExceptionMapper
{
    /// <inheritdoc />
    public ProblemDetail Map(Exception exception)
    {
        return exception switch
        {
            ArgumentException ex => new ProblemDetail
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = ex.Message
            },
            UnauthorizedAccessException ex => new ProblemDetail
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = 401,
                Detail = ex.Message
            },
            InvalidOperationException ex => new ProblemDetail
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Conflict",
                Status = 409,
                Detail = ex.Message
            },
            NotImplementedException ex => new ProblemDetail
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Not Implemented",
                Status = 501,
                Detail = ex.Message
            },
            _ => new ProblemDetail
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = exception.Message
            }
        };
    }
}
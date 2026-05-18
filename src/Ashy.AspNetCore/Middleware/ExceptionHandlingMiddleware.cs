using Ashy.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ashy.AspNetCore.Middleware;

/// <summary>
/// 全局异常处理中间件：捕获未处理异常，通过 IExceptionMapper 转为 RFC 7807 ProblemDetails 响应
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionMapper _mapper;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly bool _includeDetails;

    /// <summary>
    /// 初始化异常处理中间件
    /// </summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IExceptionMapper mapper,
        ILogger<ExceptionHandlingMiddleware> logger,
        bool includeDetails = false)
    {
        _next = next;
        _mapper = mapper;
        _logger = logger;
        _includeDetails = includeDetails;
    }

    /// <summary>
    /// 中间件入口，捕获异常并返回 RFC 7807 格式响应
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var problem = _mapper.Map(ex);
            context.Response.StatusCode = problem.Status;
            context.Response.ContentType = "application/problem+json";

            var detail = _includeDetails ? problem.Detail : null;

            await context.Response.WriteAsJsonAsync(new
            {
                type = problem.Type ?? "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = problem.Title,
                status = problem.Status,
                detail,
                instance = problem.Instance ?? context.Request.Path
            });
        }
    }
}
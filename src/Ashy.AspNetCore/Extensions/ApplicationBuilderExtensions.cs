using Ashy.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Ashy.AspNetCore.Extensions;

/// <summary>
/// IApplicationBuilder 扩展，注册 Ashy 中间件管道
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 注册 Ashy 中间件（异常处理等）
    /// </summary>
    public static IApplicationBuilder UseAshy(this IApplicationBuilder app, bool includeExceptionDetails = false)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>(includeExceptionDetails);
        return app;
    }
}
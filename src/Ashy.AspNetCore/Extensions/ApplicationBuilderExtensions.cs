using Microsoft.AspNetCore.Builder;

namespace Ashy.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAshy(this IApplicationBuilder app)
    {
        return app;
    }
}
using Microsoft.AspNetCore.Diagnostics;

namespace Adherium.Adherence.Api.Infrastructure;

internal sealed class BadRequestExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        httpContext.Response.StatusCode = badRequest.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = badRequest.StatusCode,
                Title = "Invalid request body.",
                Detail = "The request body could not be read. Ensure it is valid JSON and includes all required fields.",
            },
        });
    }
}

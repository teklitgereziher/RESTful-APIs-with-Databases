using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace AzureCosmos.CRUD.WebAPI.Middlewares
{
  public class RequestCorrelationMiddleware
  {
    private readonly RequestDelegate next;
    private const string correlationIdHeader = "X-Correlation-Id";

    public RequestCorrelationMiddleware(RequestDelegate next) => this.next = next;

    /// <summary>
    /// This method contains the middleware logic.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="correlationIdGenerator"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
    {
      var correlationId = GetCorrelationId(context, correlationIdGenerator);
      using (LogContext.PushProperty("X-Correlation-Id", correlationId.ToString()))
      {
        // Add CorrelationId to the response headers       
        AddCorrelationIdHeaderToResponse(context, correlationId);

        await next(context);
      }
    }

    private static StringValues GetCorrelationId(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
    {
      if (context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId))
      {
        correlationIdGenerator.Set(correlationId);
        return correlationId;
      }
      else
      {
        return correlationIdGenerator.Get();
      }
    }

    private static void AddCorrelationIdHeaderToResponse(HttpContext context, StringValues correlationId)
        => context.Response.OnStarting(() =>
        {
          context.Response.Headers.Append(correlationIdHeader, correlationId.ToString());
          return Task.CompletedTask;
        });
  }
}

namespace AzureCosmos.CRUD.WebAPI.Middlewares
{
  public sealed class RequestTracingMiddleware
  {
    private readonly RequestDelegate next;
    private readonly ILogger<RequestTracingMiddleware> logger;

    public RequestTracingMiddleware(
      RequestDelegate next,
      ILogger<RequestTracingMiddleware> logger)
    {
      this.next = next;
      this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var traceId = Guid.NewGuid().ToString();

      // Add TraceId to the logging scope
      using (logger.BeginScope(new Dictionary<string, object>
      {
        ["CorrelationId"] = traceId
      }))
      {
        // Add correlationid to the response
        context.Response.Headers.Append("CorrelationId", traceId);

        await next(context);
      }

      //using (LogContext.PushProperty("CorrelationId", traceId))
      //{
      //  // Add TraceId to the response headers
      //  context.Response.Headers["CorrelationId"] = traceId;
      //  await next(context);
      //}
    }
  }
}

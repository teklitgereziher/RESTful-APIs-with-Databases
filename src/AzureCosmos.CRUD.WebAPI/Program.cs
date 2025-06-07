using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.WebAPI.Configurations;
using AzureCosmos.CRUD.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Identity.Web;
using Polly;
using Serilog;

namespace AzureCosmos.CRUD.WebAPI
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration()
          .WriteTo.Console().CreateLogger();
      try
      {
        Log.Information("Starting up the application");

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));
        var dbSettings = builder.Configuration.GetSection("CosmosSettings").Get<CosmosSettings>();

        // Add services to the container.
        builder.Services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("CosmosSettings"));
        builder.Services.AddCosmosClient(dbSettings, builder.Environment.IsDevelopment());
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Configure a singleton HttpClient with resilience pipelines
        builder.Services.AddSingleton(sp =>
        {
          var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
              .AddRetry(new HttpRetryStrategyOptions
              {
                BackoffType = DelayBackoffType.Exponential,
                MaxRetryAttempts = 3
              }).Build();

          var resilienceHandler = new ResilienceHandler(retryPipeline)
          {
            InnerHandler = new SocketsHttpHandler
            {
              PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            },
          };

          return new HttpClient(resilienceHandler);
        });

        // Add authentication and authorization
        // We use the JSON Web Token (JWT) Bearer scheme as the default authentication mechanism.
        // Use the AddAuthentication method to register the JWT bearer scheme.
        // This validates the token and enables your web API to be protected using the Microsoft identity platform. (first option)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // This enables the web API to be protected using the Microsoft identity platform. (second option)
        //builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration.GetSection("AzureAd"));

        var app = builder.Build();

        app.UseMiddleware<RequestTracingMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
          app.MapOpenApi();
          app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
        }

        app.UseHttpsRedirection();

        //app.UseMiddleware<RequestTracingMiddleware>();
        app.UseSerilogRequestLogging();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Application start-up failed");
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }
  }
}

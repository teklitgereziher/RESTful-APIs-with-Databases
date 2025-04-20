using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.WebAPI.Configurations;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace AzureCosmos.CRUD.WebAPI
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var dbSettings = builder.Configuration.GetSection("CosmosSettings").Get<CosmosSettings>();

      // Add services to the container.
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

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();


      app.MapControllers();

      app.Run();
    }
  }
}

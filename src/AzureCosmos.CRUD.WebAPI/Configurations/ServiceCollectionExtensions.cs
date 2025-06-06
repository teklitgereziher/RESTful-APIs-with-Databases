using Azure.Identity;
using AzureCosmos.CRUD.DataAccess.Config;
using Microsoft.Azure.Cosmos;

namespace AzureCosmos.CRUD.WebAPI.Configurations
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddCosmosClient(this IServiceCollection services, CosmosSettings dbSettings, bool isDevelopment)
    {
      if (isDevelopment)
      {
        services.AddSingleton(sp => new CosmosClient(
          dbSettings.ConnectionString,
          new()
          {
            ConnectionMode = ConnectionMode.Gateway,
            SerializerOptions = new CosmosSerializationOptions
            {
              PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            MaxRetryAttemptsOnRateLimitedRequests = 5,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
            AllowBulkExecution = true
          }));
      }
      else
      {
        services.AddSingleton(sp => new CosmosClient(
          dbSettings.ResourceEndpoint, new ClientSecretCredential(
          dbSettings.TenantId, dbSettings.ClientId, dbSettings.ClientSecret)));
      }

      return services;
    }
  }
}

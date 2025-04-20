using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.WebAPI;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Testcontainers.CosmosDb;

namespace AzureCosmos.CRUD.IntegrationTests.Setup
{
  public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
  {
    CosmosClient cosmosClient;
    private static readonly IOutputConsumer outputConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
    private CosmosDbContainer cosmosDbContainer = new CosmosDbBuilder()
      .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
      .WithName("cosmosdb-test-emulator")
      .WithPortBinding(8081, 8081)
      .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
      .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
      .WithOutputConsumer(outputConsumer)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8081)
      .UntilMessageIsLogged("Started"))
      .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureAppConfiguration((context, config) =>
      {
        string configPath = Path.Combine(Directory.GetParent(
          Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Setup", "appsettings.json");
        config.AddJsonFile(configPath);
      });

      base.ConfigureWebHost(builder);
      builder.ConfigureTestServices(services =>
      {
        services.RemoveAll<CosmosClient>();
        services.AddSingleton(sp => new CosmosClient(cosmosDbContainer.GetConnectionString(), new CosmosClientOptions
        {
          ConnectionMode = ConnectionMode.Gateway,
          HttpClientFactory = () =>
          {
            HttpMessageHandler httpMessageHandler = new HttpClientHandler()
            {
              ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };

            return new HttpClient(httpMessageHandler);
          },
          SerializerOptions = new CosmosSerializationOptions()
          {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
          }
        }));
      });
    }

    /// <summary>
    /// Initializes the container
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
      await cosmosDbContainer.StartAsync();
      var dbSettings = Services.GetRequiredService<IOptions<CosmosSettings>>().Value;
      cosmosClient = Services.CreateScope().ServiceProvider.GetRequiredService<CosmosClient>();
      var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbSettings.DatabaseName);
      await database.Database.CreateContainerIfNotExistsAsync(dbSettings.ContainerName, "/id");
    }

    /// <summary>
    /// Disposes the container when the test is completed
    /// </summary>
    /// <returns></returns>
    async Task IAsyncLifetime.DisposeAsync()
    {
      await cosmosDbContainer.DisposeAsync();
    }

    //public void ClearTables()
    //{
    //  var userDbSet = dbContext.Set<User>();
    //  userDbSet.RemoveRange(userDbSet);
    //  dbContext.SaveChanges();
    //}
  }
}

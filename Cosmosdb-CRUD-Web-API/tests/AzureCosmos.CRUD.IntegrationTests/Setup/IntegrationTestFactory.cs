using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.WebAPI;
using AzureCosmos.CRUD.WebAPI.AutoMapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.CosmosDb;

namespace AzureCosmos.CRUD.IntegrationTests.Setup
{
  public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
  {
    private IConfiguration configuration;
    private readonly IOutputConsumer outputConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
    private CosmosDbContainer cosmosDbContainer;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureAppConfiguration((context, config) =>
      {
        var configPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Setup", "appsettings.json");
        config.AddJsonFile(configPath, optional: false, reloadOnChange: true);
      });
      base.ConfigureWebHost(builder);
      builder.ConfigureTestServices(services =>
      {
        services.AddSingleton<IConfiguration>(configuration);
        var dbSettings = configuration.GetSection("CosmosSettings").Get<CosmosSettings>();
        services.Configure<CosmosSettings>(configuration.GetSection("CosmosSettings"));
        services.AddSingleton(AutoMapperConfig.Mapper);
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

    public async Task InitializeAsync()
    {
      cosmosDbContainer = new CosmosDbBuilder()
      .WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
      .WithName("cosmosdb-test-emulator")
      .WithPortBinding(8081, 8081)
      .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
      .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "false")
      .WithOutputConsumer(outputConsumer)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8081)
      .UntilMessageIsLogged("Started"))
      .Build();

      var dbSettings = configuration.GetSection("CosmosSettings").Get<CosmosSettings>();

      await cosmosDbContainer.StartAsync();


      var factory = new IntegrationTestFactory();
      var cosmosClient = factory.Services.CreateScope().ServiceProvider.GetRequiredService<CosmosClient>();
      var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbSettings.DatabaseName);
      await database.Database.CreateContainerIfNotExistsAsync(dbSettings.ContainerName, "/id");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
      await cosmosDbContainer.DisposeAsync();
    }
  }
}

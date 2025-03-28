using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.WebAPI.AutoMapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.CosmosDb;

namespace AzureCosmos.CRUD.IntegrationTests.Setup
{
  [CollectionDefinition(nameof(TestFixture))]
  public class BaseIntegrationTest : ICollectionFixture<TestFixture> { }

  public class TestFixture : IAsyncLifetime
  {
    private IConfiguration configuration;
    private readonly IOutputConsumer outputConsumer = Consume.RedirectStdoutAndStderrToStream(new MemoryStream(), new MemoryStream());
    private CosmosDbContainer cosmosDbContainer;

    public IServiceProvider ServiceProvider;

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

      string configPath = Path.Combine(Directory.GetParent(
          Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Setup", "appsettings.json");

      configuration = new ConfigurationBuilder()
        .AddJsonFile(configPath, optional: false, reloadOnChange: true)
        .Build();

      var builder = Host.CreateApplicationBuilder();

      builder.Services.AddSingleton(configuration);

      var dbSettings = configuration.GetSection("CosmosSettings").Get<CosmosSettings>();

      await cosmosDbContainer.StartAsync();

      builder.Services.RegisterServices(configuration, cosmosDbContainer);
      ServiceProvider = builder.Services.BuildServiceProvider();

      var cosmosClient = ServiceProvider.GetRequiredService<CosmosClient>();
      var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(dbSettings.DatabaseName);
      await database.Database.CreateContainerIfNotExistsAsync(dbSettings.ContainerName, "/id");
    }

    public async Task DisposeAsync()
    {
      await cosmosDbContainer.DisposeAsync();
    }
  }

  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection RegisterServices(
      this IServiceCollection services,
      IConfiguration configuration,
      CosmosDbContainer cosmosDbContainer)
    {
      var dbSettings = configuration.GetSection("CosmosSettings").Get<CosmosSettings>();
      services.Configure<CosmosSettings>(configuration.GetSection("CosmosSettings"));
      services.AddTransient<IBookRepository, BookRepository>();
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

      return services;
    }
  }

}

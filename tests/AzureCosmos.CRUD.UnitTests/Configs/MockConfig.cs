using AzureCosmos.CRUD.DataAccess.Config;
using Microsoft.Extensions.Options;

namespace AzureCosmos.CRUD.UnitTests.Configs
{
  public class MockConfig
  {
    public static IOptions<CosmosSettings> options
    {
      get
      {
        var cosmosSettings = new CosmosSettings
        {
          ResourceEndpoint = "https://localhost:8081",
          DatabaseName = "TestDatabase",
          ContainerName = "TestContainer"
        };
        return Options.Create(cosmosSettings);
      }
    }
  }
}

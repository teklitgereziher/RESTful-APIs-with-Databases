using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AzureCosmos.CRUD.IntegrationTests.SetupPostgres
{
  public static class WebApplicationFactoryExtensions
  {

    private const string testAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJvaWQiOiJ1c2VyMSJ9.abc123";
    public static WebApplicationFactory<T> WithAuthentication<T>(
      this WebApplicationFactory<T> factory) where T : class
    {
      return AddAuthentication<T, MockAuthenticationHandler>(factory);
    }

    private static WebApplicationFactory<T> AddAuthentication<T, TAuthenticationHandler>(
      this WebApplicationFactory<T> factory)
        where T : class
        where TAuthenticationHandler : MockAuthenticationHandler
    {
      return factory.WithWebHostBuilder(builder =>
      {
        builder.ConfigureTestServices(services =>
        {
          services.AddAuthentication("Test")
              .AddScheme<AuthenticationSchemeOptions, TAuthenticationHandler>("Test", options => { });
        });
      });
    }

    public static WebApplicationFactory<TFactory> WithService<TFactory, TService>(
      this WebApplicationFactory<TFactory> factory,
      Func<IServiceProvider, TService> implementationFactory)
      where TService : class
      where TFactory : class
    {
      return factory.WithWebHostBuilder(builder =>
      {
        builder.ConfigureTestServices(services =>
        {
          services.AddScoped(implementationFactory);
        });
      });
    }

    public static HttpClient CreateAndConfigureClient<T>(
      this WebApplicationFactory<T> factory) where T : class
    {
      var client = factory.CreateClient(new WebApplicationFactoryClientOptions
      {
        AllowAutoRedirect = false
      });

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testAccessToken);

      return client;
    }
  }

}

using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.WebAPI.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureCosmos.CRUD.IntegrationTests.Setup
{
  public class ApiBaseIntegrationTest : IClassFixture<IntegrationTestFactory>
  {
    protected readonly IBookRepository bookRepository;
    protected readonly ILogger<BooksController> logger;
    protected readonly HttpClient clientWithAuth;
    protected readonly HttpClient clientWithoutAuth;

    public ApiBaseIntegrationTest(IntegrationTestFactory factory)
    {
      clientWithoutAuth = factory.CreateClient();
      clientWithAuth = factory.WithAuthentication()
        .CreateAndConfigureClient();

      var serviceScope = factory.Services.CreateScope();
      bookRepository = serviceScope.ServiceProvider.GetService<IBookRepository>();
      logger = serviceScope.ServiceProvider.GetService<ILogger<BooksController>>();
    }
  }
}

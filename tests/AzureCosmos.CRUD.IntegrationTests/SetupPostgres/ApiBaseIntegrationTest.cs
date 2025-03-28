using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Postgres.CRUD.DataAccess.Repository;
using Postgres.CRUD.WebAPI.Controllers;

namespace AzureCosmos.CRUD.IntegrationTests.SetupPostgres
{
  public class ApiBaseIntegrationTest : IClassFixture<IntegrationTestFactory>
  {
    protected readonly IBookRepository bookRepository;
    protected readonly ILogger<BookController> logger;
    protected readonly HttpClient clientWithAuth;
    protected readonly HttpClient clientWithoutAuth;

    public ApiBaseIntegrationTest(IntegrationTestFactory factory)
    {
      clientWithoutAuth = factory.CreateClient();
      clientWithAuth = factory.WithAuthentication()
        .CreateAndConfigureClient();

      bookRepository = factory.Services.GetRequiredService<IBookRepository>();
      logger = factory.Services.GetRequiredService<ILogger<BookController>>();
    }
  }
}

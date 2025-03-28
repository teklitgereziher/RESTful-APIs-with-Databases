using Microsoft.Extensions.DependencyInjection;
using Postgres.CRUD.DataAccess.Repository;

namespace AzureCosmos.CRUD.IntegrationTests.SetupPostgres
{
  public class BaseIntegrationTest<T> : IClassFixture<IntegrationTestFactory>
  {
    private readonly IServiceScope serviceScope;
    protected readonly T Sut;
    protected readonly IBookRepository bookRepository;

    public BaseIntegrationTest(IntegrationTestFactory factory)
    {
      serviceScope = factory.Services.CreateScope();
      Sut = serviceScope.ServiceProvider.GetRequiredService<T>();
      bookRepository = serviceScope.ServiceProvider.GetRequiredService<IBookRepository>();
    }
  }
}

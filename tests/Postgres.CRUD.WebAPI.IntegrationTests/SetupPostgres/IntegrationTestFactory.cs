using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Postgres.CRUD.DataAccess.DatabaseContext;
using Postgres.CRUD.DataAccess.Models;
using Testcontainers.PostgreSql;

namespace Postgres.CRUD.WebAPI.IntegrationTests.SetupPostgres
{
  public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
  {
    protected BookDbContext dbContext;
    private PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
      .WithImage("postgres:latest")
      .WithDatabase("testdb")
      .WithUsername("admin")
      .WithPassword("admin")
      .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureAppConfiguration((context, config) =>
      {
        string configPath = Path.Combine(Directory.GetParent(
          Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Setup", "appsettings.json");
        config.AddJsonFile(configPath);
      });

      var conString = dbContainer.GetConnectionString();
      base.ConfigureWebHost(builder);
      builder.ConfigureTestServices(services =>
      {
        services.RemoveAll(typeof(DbContextOptions<BookDbContext>));
        services.AddDbContext<BookDbContext>(options =>
        {
          options.UseNpgsql(conString);
        });
      });
    }

    /// <summary>
    /// Initializes the container
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
      await dbContainer.StartAsync();
      dbContext = Services.CreateScope().ServiceProvider.GetRequiredService<BookDbContext>();
      dbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Disposes the container when the test is completed
    /// </summary>
    /// <returns></returns>
    async Task IAsyncLifetime.DisposeAsync()
    {
      await dbContainer.DisposeAsync();
    }

    public void ClearTables()
    {
      var userDbSet = dbContext.Set<Book>();
      userDbSet.RemoveRange(userDbSet);
      dbContext.SaveChanges();
    }
  }
}

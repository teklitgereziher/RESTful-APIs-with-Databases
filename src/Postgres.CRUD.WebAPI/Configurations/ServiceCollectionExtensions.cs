using Microsoft.EntityFrameworkCore;
using Postgres.CRUD.DataAccess.Configs;
using Postgres.CRUD.DataAccess.DatabaseContext;

namespace Postgres.CRUD.WebAPI.Configurations
{
  public static class ServiceCollectionExtensions
  {
    public static void AddPostgres(
      this IServiceCollection services,
      PostgresSettings pgsqlSettings,
      bool isDevelopment)
    {
      if (isDevelopment)
      {
        services.AddDbContext<BookDbContext>(options =>
        {
          options.UseNpgsql(pgsqlSettings.DevConnectionString);
        });
      }
      else
      {
        services.AddDbContext<BookDbContext>((sp, options) =>
        {
          options.UseNpgsql(pgsqlSettings.ConnectionString);
          options.AddInterceptors(sp.GetRequiredService<DbAuthInterceptor>());
        });
      }
    }
  }
}

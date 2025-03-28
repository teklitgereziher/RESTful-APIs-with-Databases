using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Postgres.CRUD.DataAccess.Configs;
using Postgres.CRUD.DataAccess.DatabaseContext;

namespace Postgres.CRUD.DataAccess.Configurations
{
  public static class ServiceCollectionExtensions
  {
    public static void AddPostgres(
      this IServiceCollection services,
      PgSqlSettings pgsqlSettings,
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
          options.UseNpgsql(pgsqlSettings.PgSqlConnectionString);
          options.AddInterceptors(sp.GetRequiredService<DbAuthInterceptor>());
        });
      }
    }
  }
}

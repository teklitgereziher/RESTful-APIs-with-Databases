using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Postgres.CRUD.DataAccess.Configs;

namespace Postgres.CRUD.DataAccess.DatabaseContext
{
  public class DbAuthInterceptor : DbConnectionInterceptor
  {
    private readonly PgSqlSettings pgSqlSettings;
    private readonly EntraIdSettings entraIdSettings;

    public DbAuthInterceptor(IOptions<PgSqlSettings> pgSqlSettings, IOptions<EntraIdSettings> entraIdSettings)
    {
      this.pgSqlSettings = pgSqlSettings.Value;
      this.entraIdSettings = entraIdSettings.Value;
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData)
    {
      base.ConnectionOpened(connection, eventData);
    }
  }
}

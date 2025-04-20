using System.Data.Common;
using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Npgsql;
using Postgres.CRUD.DataAccess.Configs;

namespace Postgres.CRUD.DataAccess.DatabaseContext
{
  public class DbAuthInterceptor : DbConnectionInterceptor
  {
    private readonly PostgresSettings pgSqlSettings;
    private readonly ClientSecretCredential pgTokenProvider;

    public DbAuthInterceptor(
      IOptions<PostgresSettings> pgSqlSettings,
      ClientSecretCredential clientSecretCredential)
    {
      this.pgSqlSettings = pgSqlSettings.Value;
      pgTokenProvider = clientSecretCredential;
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
      DbConnection connection,
      ConnectionEventData eventData,
      InterceptionResult result,
      CancellationToken cancellationToken = default)
    {
      AccessToken accessToken = await GetFreshTokenAsync();
      connection.ConnectionString = pgSqlSettings.ConnectionString;
      var pgSqlConnection = (NpgsqlConnection)connection;
      pgSqlConnection.ConnectionString += accessToken.Token;

      return result;
    }

    private async ValueTask<AccessToken> GetFreshTokenAsync()
    {
      var accessToken = await pgTokenProvider.GetTokenAsync(
          new TokenRequestContext(pgSqlSettings.Scopes),
          CancellationToken.None);

      return accessToken;
    }
  }
}

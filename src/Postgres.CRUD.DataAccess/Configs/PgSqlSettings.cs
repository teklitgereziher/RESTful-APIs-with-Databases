namespace Postgres.CRUD.DataAccess.Configs
{
  public class PgSqlSettings
  {
    public string PgSqlClientSecret { get; set; }
    public string[] PgSqlScopes { get; set; }
    public string PgSqlConnectionString { get; set; }
    public string DevConnectionString { get; set; }
  }
}

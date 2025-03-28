namespace Postgres.CRUD.DataAccess.Configs
{
  public class PostgresSettings
  {
    public string ClientSecret { get; set; }
    public string[] Scopes { get; set; }
    public string ConnectionString { get; set; }
    public string DevConnectionString { get; set; }
  }
}

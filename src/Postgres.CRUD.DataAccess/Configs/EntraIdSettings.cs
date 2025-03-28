namespace Postgres.CRUD.DataAccess.Configs
{
  public class EntraIdSettings
  {
    public string Instance { get; set; }
    public string TenantId { get; set; }
    public string ClientId { get; set; } // Web APP app registration clientId
    public string ServicePrincipalId { get; set; }
  }
}

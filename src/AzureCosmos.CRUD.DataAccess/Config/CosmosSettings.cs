namespace AzureCosmos.CRUD.DataAccess.Config
{
  public class CosmosSettings
  {
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string ResourceEndpoint { get; set; }
    public string DatabaseName { get; set; }
    public string ContainerName { get; set; }
    public string ConnectionString { get; set; }
  }
}

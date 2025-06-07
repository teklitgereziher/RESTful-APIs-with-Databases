namespace AzureCosmos.CRUD.WebAPI.Middlewares
{
  public class CorrelationIdGenerator : ICorrelationIdGenerator
  {
    private string correlationId = Guid.NewGuid().ToString();

    public string Get() => correlationId;

    public void Set(string correlationId) => this.correlationId = correlationId;
  }
}

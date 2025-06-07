namespace AzureCosmos.CRUD.WebAPI.Middlewares
{
  public interface ICorrelationIdGenerator
  {
    string Get();
    void Set(string correlationId);
  }
}

using AzureCosmos.CRUD.DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace AzureCosmos.CRUD.DataAccess.Repository
{
  public interface IBookRepository
  {
    Task<Book> GetBookAsync(string partyId);
    Task<List<Book>> GetBooksAsync(IReadOnlyList<(string partyId, PartitionKey partitionKey)> items);
    Task<Book> InsertOrReplaceAsync(Book customer);
  }
}

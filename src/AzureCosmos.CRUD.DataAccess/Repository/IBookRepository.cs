using AzureCosmos.CRUD.DataAccess.Models;
using Microsoft.Azure.Cosmos;

namespace AzureCosmos.CRUD.DataAccess.Repository
{
  public interface IBookRepository
  {
    Task<Book> GetBookAsync(string id);
    Task<List<Book>> GetBooksAsync(IReadOnlyList<(string bookId, PartitionKey partitionKey)> items);
    Task<Book> InsertOrReplaceAsync(Book book);
    Task<Book> UpdateBookAsync(string bookId, string bookTitle);
    Task<bool?> DeleteBookAsync(string bookId);
    Task<List<Book>> QueryBooksAsync(string bookTitle);
    Task<Book> AddBookAsync(Book book);
    Task BulkInsertAsync(int numOfBooks, string publisherId, string userId);

    Task StoreVehicleAsync(Book vehicle, string partitionKey);
    Task<List<string>> FetchDistinctDealerIdsByMainDealerIdAsync(
      string mainDealerId,
      string clientAppId,
      List<string> repairingDealerIds);
    Task<int> FetchTotalCountAsync(
      string mainDealerId,
      string clientAppId,
      List<string> repairingDealerIds,
      Dictionary<string, string> filters);
    Task<BookQueryResult> QueryBooksByPartitionKeyAndFiltersAsync(
      string bookId,
      string userId,
      List<string> bookNames,
      Dictionary<string, string> filters,
      int pageSize,
      string continuationToken);
    Task<List<Book>> GetBooksAsync(
      List<string> bookIds,
      string publisherId,
      string userId,
      List<(string property, bool decending)> orderProperties);
  }
}

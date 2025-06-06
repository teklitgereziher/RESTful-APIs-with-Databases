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
    Task BulkInsertAsync(int numOfBooks);
  }
}

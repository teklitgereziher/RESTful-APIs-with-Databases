using System.Diagnostics;
using System.Net;
using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Models;
using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureCosmos.CRUD.DataAccess.Repository
{
  public class BookRepository : IBookRepository
  {
    private readonly ILogger<BookRepository> logger;
    private readonly Container container;

    public BookRepository(
      ILogger<BookRepository> logger,
      IOptions<CosmosSettings> dbSettings,
      CosmosClient cosmosClient)
    {
      this.logger = logger;
      container = cosmosClient.GetContainer(dbSettings.Value.DatabaseName, dbSettings.Value.ContainerName);
    }

    public async Task<Book> GetBookAsync(string id)
    {
      try
      {
        var itemResult = await container.ReadItemAsync<Book>(id, new PartitionKey(id));
        return itemResult.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        logger.LogWarning(ex, "Book with bookId={BookId} not found.", id);
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Error reading Book with bookId={BookId}.", id);
      }
      catch (ArgumentNullException ex)
      {
        logger.LogError(ex, "The argument is null.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error reading customer with partyId={PartyId}.", id);
      }

      return null;
    }

    public async Task<List<Book>> GetBooksAsync(IReadOnlyList<(string bookId, PartitionKey partitionKey)> items)
    {
      try
      {
        var feedResponse = await container.ReadManyItemsAsync<Book>(items);
        return [.. feedResponse.Resource];
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Cosmos db exception while reading books.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error reading books.");
      }

      return [];
    }

    public async Task<Book> AddBookAsync(Book book)
    {
      ItemResponse<Book> bookResponse = null;
      try
      {
        bookResponse = await container.CreateItemAsync(book, new PartitionKey(book.Id));

        if (bookResponse.StatusCode == HttpStatusCode.Created)
        {
          logger.LogError("Book added successfully.");
        }
      }
      catch (CosmosException ex)
      {
        switch (ex.StatusCode)
        {
          case HttpStatusCode.TooManyRequests:
            logger.LogError(ex, "Request rate too large. Retry after: {RetryAfter} seconds.", ex.RetryAfter);
            await Task.Delay((TimeSpan)ex.RetryAfter);
            break;

          case HttpStatusCode.BadRequest:
            logger.LogError(ex, "Bad request: {Message}", ex.Message);
            break;

          default:
            logger.LogError(ex, "An error occurred: {StatusCode}, {Message}", ex.StatusCode, ex.Message);
            break;
        }
      }

      return bookResponse?.Resource;
    }

    public async Task BulkInsertAsync(int numOfBooks)
    {
      var books = GenerateFakeBooks(numOfBooks);
      var tasks = new List<Task>();
      try
      {
        Stopwatch stopwatch = Stopwatch.StartNew();
        foreach (var book in books)
        {
          //await container.CreateItemAsync(book, new PartitionKey(book.Id));
          tasks.Add(container.CreateItemAsync(book, new PartitionKey(book.Id))
          .ContinueWith(itemResponse =>
          {
            if (!itemResponse.IsCompletedSuccessfully)
            {
              AggregateException innerExceptions = itemResponse.Exception.Flatten();
              if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
              {
                logger.LogError(cosmosException, "Error adding book with bookId={BookId} {StatusCode} ({Message}).",
                  book.Id,
                  cosmosException.StatusCode,
                  cosmosException.Message);
              }
              else
              {
                logger.LogError(innerExceptions, "Error adding book with bookId={BookId}.", book.Id);
              }
            }
          }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();
        logger.LogInformation("Bulk insert completed in {ElapsedMilliseconds} ms for {Count} books.",
          stopwatch.ElapsedMilliseconds,
          numOfBooks);
      }
      catch (CosmosException ex)
      {
        switch (ex.StatusCode)
        {
          case HttpStatusCode.TooManyRequests:
            logger.LogError(ex, "Request rate too large. Retry after: {RetryAfter} seconds.", ex.RetryAfter);
            await Task.Delay((TimeSpan)ex.RetryAfter);
            break;

          case HttpStatusCode.BadRequest:
            logger.LogError(ex, "Bad request: {Message}", ex.Message);
            break;

          default:
            logger.LogError(ex, "An error occurred: {StatusCode}, {Message}", ex.StatusCode, ex.Message);
            break;
        }
      }
    }

    public async Task BulkInsertAsync2(int numOfBooks)
    {
      var books = GenerateFakeBooks(numOfBooks);
      var tasks = new List<Task>();
      Stopwatch stopwatch = Stopwatch.StartNew();
      foreach (var book in books)
      {
        tasks.Add(container.CreateItemAsync(book, new PartitionKey(book.Id)));
      }
      try
      {
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        logger.LogInformation("Bulk insert completed in {ElapsedMilliseconds} ms for {Count} books.",
          stopwatch.ElapsedMilliseconds,
          numOfBooks);
      }
      catch (AggregateException ex)
      {
        if (ex.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
        {
          logger.LogError(cosmosException, "Error adding book {StatusCode} ({Message}).",
            cosmosException.StatusCode,
            cosmosException.Message);
        }
        else
        {
          logger.LogError(ex, "Error adding book.");
        }
      }
    }

    public async Task<Book> InsertOrReplaceAsync(Book book)
    {
      try
      {
        var itemResult = await container.UpsertItemAsync(book, new PartitionKey(book?.Id));
        if (itemResult.StatusCode == HttpStatusCode.Created)
        {
          logger.LogInformation("Book with bookId={BookId} created.", book?.Id);
        }
        else if (itemResult.StatusCode == HttpStatusCode.OK)
        {
          logger.LogInformation("Book with bookId={BookId} updated.", book?.Id);
        }
        return itemResult.Resource;
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Error inserting or replacing book with bookId={BookId}.", book?.Id);
      }
      catch (ArgumentNullException ex)
      {
        logger.LogError(ex, "The argument to add or update is null.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error when inserting or replacing book.");
      }

      return null;
    }

    public async Task<Book> UpdateBookAsync(string bookId, string bookTitle)
    {
      var book = await GetBookAsync(bookId);
      if (book == null)
      {
        logger.LogWarning("Book with bookId={BookId} not found.", bookId);
        return null;
      }

      book.Title = bookTitle;
      var result = await container.ReplaceItemAsync<Book>(book, bookId, new PartitionKey(bookId));
      if (result.StatusCode == HttpStatusCode.OK)
      {
        logger.LogInformation("Book with bookId={BookId} updated.", bookId);
      }
      else
      {
        logger.LogError("Failed to update book with bookId={BookId}. Satus code: {StatusCode}", bookId, result.StatusCode);
      }

      return result.Resource;
    }

    public async Task<bool?> DeleteBookAsync(string bookId)
    {
      try
      {
        await container.DeleteItemAsync<Book>(bookId, new PartitionKey(bookId));
        logger.LogInformation("Book with bookId={BookId} deleted.", bookId);

        return true;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        logger.LogWarning(ex, "Book with bookId={BookId} not found.", bookId);
        return null;
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Error deleting book with bookId={BookId}.", bookId);
      }
      catch (ArgumentNullException ex)
      {
        logger.LogError(ex, "The argument to delete is null.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error when deleting book.");
      }

      return false;
    }

    public async Task<List<Book>> QueryBooksAsync(string bookTitle)
    {
      string sqlQueryText = $"SELECT * FROM c WHERE c.title = '{bookTitle}'";

      var queryDefinition = new QueryDefinition(sqlQueryText);
      FeedIterator<Book> queryResultSetIterator = container.GetItemQueryIterator<Book>(queryDefinition);

      var books = new List<Book>();

      while (queryResultSetIterator.HasMoreResults)
      {
        FeedResponse<Book> currentResultSet = await queryResultSetIterator.ReadNextAsync();
        foreach (Book book in currentResultSet)
        {
          books.Add(book);
        }
      }

      return books;
    }

    public static List<Book> GenerateFakeBooks(int count)
    {
      var authorFaker = new Faker<Author>()
          .RuleFor(a => a.Name, f => f.Name.FullName())
          .RuleFor(a => a.Phone, f => f.Phone.PhoneNumber())
          .RuleFor(a => a.Email, f => f.Internet.Email())
          .RuleFor(a => a.Website, f => f.Internet.Url());

      var publisherFaker = new Faker<Publisher>()
          .RuleFor(p => p.Name, f => f.Company.CompanyName())
          .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
          .RuleFor(p => p.Email, f => f.Internet.Email())
          .RuleFor(p => p.Website, f => f.Internet.Url());

      var bookFaker = new Faker<Book>()
          .RuleFor(b => b.Id, f => f.Random.Guid().ToString())
          .RuleFor(b => b.Title, f => f.Lorem.Sentence(10))
          .RuleFor(b => b.Year, f => f.Date.Past(20).Year)
          .RuleFor(b => b.Price, f => f.Finance.Amount(5, 200))
          .RuleFor(b => b.Author, f => authorFaker.Generate())
          .RuleFor(b => b.Publisher, f => publisherFaker.Generate());

      return bookFaker.Generate(count);
    }
  }
}

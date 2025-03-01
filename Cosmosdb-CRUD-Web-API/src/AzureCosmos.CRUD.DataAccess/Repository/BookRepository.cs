using System.Net;
using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Models;
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

    public async Task<Book> GetBookAsync(string partyId)
    {
      try
      {
        var itemResult = await container.ReadItemAsync<Book>(partyId, new PartitionKey(partyId));
        return itemResult.Resource;
      }
      catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
      {
        logger.LogWarning("Customer with partyId={PartyId} not found.", partyId);
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Error reading customer with partyId={PartyId}.", partyId);
      }
      catch (ArgumentNullException ex)
      {
        logger.LogError(ex, "The argument is null.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error reading customer with partyId={PartyId}.", partyId);
      }

      return null;
    }

    public async Task<List<Book>> GetBooksAsync(IReadOnlyList<(string partyId, PartitionKey partitionKey)> items)
    {
      try
      {
        var feedResponse = await container.ReadManyItemsAsync<Book>(items);
        return feedResponse.Resource.ToList();
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Cosmos db exception while reading customers.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error reading customers.");
      }

      return [];
    }

    public async Task<Book> InsertOrReplaceAsync(Book customer)
    {
      try
      {
        var itemResult = await container.UpsertItemAsync(customer, new PartitionKey(customer?.ISBN));
        if (itemResult.StatusCode == HttpStatusCode.Created)
        {
          logger.LogInformation("Customer with partyId={PartyId} created.", customer.ISBN);
        }
        else if (itemResult.StatusCode == HttpStatusCode.OK)
        {
          logger.LogInformation("Customer with partyId={PartyId} updated.", customer.ISBN);
        }
        return itemResult.Resource;
      }
      catch (CosmosException ex)
      {
        logger.LogError(ex, "Error inserting or replacing customer with partyId={PartyId}.", customer.ISBN);
      }
      catch (ArgumentNullException ex)
      {
        logger.LogError(ex, "The argument to add or update is null.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error when inserting or replacing customer data.");
      }

      return null;
    }
  }
}

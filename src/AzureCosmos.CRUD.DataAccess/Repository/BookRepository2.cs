using System.Linq.Expressions;
using AzureCosmos.CRUD.DataAccess.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace AzureCosmos.CRUD.DataAccess.Repository
{
  public partial class BookRepository : IBookRepository
  {
    public async Task StoreVehicleAsync(Book book, string partitionKey)
    {
      try
      {
        var updatedPartitionKey = new PartitionKeyBuilder().Add(book.BookId).Add(book.UserId).Build();
        var res = await container.UpsertItemAsync(book, updatedPartitionKey);

        logger.LogInformation("Stored vehicle with id: {Id} and partition key: {PartitionKeys} Request charge: {RequestCharge}.",
        book.BookId, partitionKey, res.RequestCharge);
      }
      catch (Exception ex)
      {
        HandleCosmosException(ex, "storing vehicle");
      }
    }

    public async Task<List<string>> FetchDistinctDealerIdsByMainDealerIdAsync(
      string bookId,
      string userId,
      List<string> bookNames)
    {
      try
      {
        var requestOptions = new QueryRequestOptions
        {
          MaxItemCount = -1,
          PartitionKey = new PartitionKeyBuilder().Add(bookId).Add(userId).Build()
        };

        var queryable = container.GetItemLinqQueryable<Book>
        (
          allowSynchronousQueryExecution: true,
          requestOptions: requestOptions
        ).Where(c => bookNames.Contains(c.Title));

        var distinctIds = queryable.Select(c => c.Title).Distinct();
        var iterator = CreateFeedIterator(distinctIds);
        var found = new List<string>();

        while (iterator.HasMoreResults)
        {
          var resp = await iterator.ReadNextAsync();
          found.AddRange(resp);
        }

        logger.LogDebug("Fetched {DealerIds} distinct dealer ids for main dealer id: " +
          "{MainDealerId} and client app id: {ClientAppId}.",
        found.Distinct().ToList(), bookId, userId);

        return [.. found.Distinct()];

      }
      catch (Exception ex)
      {
        HandleCosmosException(ex, "executing FetchDistinctDealerIdsByMainDealerIdAsync query");
        throw;
      }
    }

    public async Task<int> FetchTotalCountAsync(
      string bookId,
      string userId,
      List<string> repairingDealerIds,
      Dictionary<string, string> filters)
    {
      try
      {
        var partitionKey = new PartitionKeyBuilder().Add(bookId).Add(userId).Build();
        var queryable = CreateVehicleQueryable(partitionKey);

        var filteredQuery = ApplyFilters(queryable, repairingDealerIds, filters);

        logger.LogDebug("Counting vehicles with partition key: {PartitionKey} and repairing dealer ids: {RepairingDealerIds}.",
          partitionKey, repairingDealerIds);

        var count = await CountAsync(filteredQuery);
        logger.LogDebug("Count completed. Total count: {TotalCount}.", count);

        return count;
      }
      catch (Exception ex)
      {
        HandleCosmosException(ex, "counting vehicles");
        throw;
      }
    }

    public async Task<BookQueryResult> QueryBooksByPartitionKeyAndFiltersAsync(
        string mainDealerId,
        string clientAppId,
        List<string> repDealerIds,
        Dictionary<string, string> filters,
        int pageSize,
        string continuationToken)
    {
      try
      {
        var partitionKey = new PartitionKeyBuilder().Add(mainDealerId).Add(clientAppId).Build();

        // Queryable for paging (with continuation token and page size)
        var pagedQueryable = CreateVehicleQueryable(partitionKey, pageSize, continuationToken);
        var pagedQuery = ApplyFilters(pagedQueryable, repDealerIds, filters);

        logger.LogDebug("Querying with partition key: {MainDealerId} and repairing dealer ids: {RepDealerIds}",
          partitionKey,
          repDealerIds);

        using var feed = CreateFeedIterator(pagedQuery);

        var results = new List<Book>();
        string newContinuationToken = null;
        double consumedRUs = 0;

        if (feed.HasMoreResults)
        {
          var response = await feed.ReadNextAsync();
          results.AddRange(response);
          newContinuationToken = response.ContinuationToken;
          consumedRUs += response.RequestCharge;
        }

        logger.LogDebug("Query completed. Continuation token: {NewContinuationToken}, Consumed RUs: {ConsumedRUs}.",
          newContinuationToken ?? "None", consumedRUs);

        return new BookQueryResult(results)
        {
          ContinuationToken = newContinuationToken,
        };
      }
      catch (Exception ex)
      {
        HandleCosmosException(ex, "querying vehicles by partition key with filters");
        throw;
      }
    }

    public static IQueryable<Book> ApplyFilters(
    IQueryable<Book> query,
    List<string> repairingDealerIds,
    Dictionary<string, string> filters)
    {
      var q = query.Where(c => repairingDealerIds.Contains(c.Title));
      return filters.Aggregate(q, (current, kv) => current.Where(CreateEqualsPredicate(kv.Key, kv.Value)));
    }

    public static IQueryable<T> ApplyDynamicOrdering<T>(
    IQueryable<T> source,
    List<(string Property, bool Descending)> orderFields)
    {
      if (orderFields == null || orderFields.Count == 0)
        return source;

      IOrderedQueryable<T> orderedQuery = null;
      var parameter = Expression.Parameter(typeof(T), "x");

      foreach (var (property, descending) in orderFields)
      {
        var propertyAccess = Expression.PropertyOrField(parameter, property);
        var orderByExp = Expression.Lambda(propertyAccess, parameter);

        string methodName;
        if (orderedQuery == null)
          methodName = descending ? "OrderByDescending" : "OrderBy";
        else
          methodName = descending ? "ThenByDescending" : "ThenBy";

        var resultExp = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), propertyAccess.Type },
            orderedQuery == null ? source.Expression : orderedQuery.Expression,
            Expression.Quote(orderByExp));

        orderedQuery = (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExp);
      }

      return orderedQuery ?? source;
    }

    private static Expression<Func<Book, bool>> CreateEqualsPredicate(
      string propertyName,
      string propertyValue)
    {
      var param = Expression.Parameter(typeof(Book), "c");
      var key = Expression.Property(param, propertyName);
      var value = Expression.Constant(propertyValue);
      var body = Expression.Equal(key, value);
      return Expression.Lambda<Func<Book, bool>>(body, param);
    }

    private IQueryable<Book> CreateVehicleQueryable(
      PartitionKey partitionKey,
      int? pageSize = null,
      string continuationToken = null)
    {
      var requestOptions = new QueryRequestOptions
      {
        PartitionKey = partitionKey,
        MaxItemCount = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : null
      };
      return container.GetItemLinqQueryable<Book>(
        allowSynchronousQueryExecution: true,
        continuationToken: continuationToken,
        requestOptions: requestOptions
      );
    }

    /// <summary>
    /// Factory for creating a FeedIterator from an IQueryable of strings. Override in tests.
    /// </summary>
    protected virtual FeedIterator<string> CreateFeedIterator(IQueryable<string> queryable)
    {
      return queryable.ToFeedIterator();
    }

    protected virtual FeedIterator<Book> CreateFeedIterator(IQueryable<Book> queryable)
    {
      return queryable.ToFeedIterator();
    }

    /// <summary>
    /// Virtual method for counting matching entities. Override in tests.
    /// </summary>
    protected virtual async Task<int> CountAsync(IQueryable<Book> queryable)
    {
      var response = await queryable.CountAsync();
      return response.Resource;
    }

    private void HandleCosmosException(Exception ex, string operationDescription)
    {
      if (ex is CosmosException cosmosEx)
      {
        switch (cosmosEx.StatusCode)
        {
          case System.Net.HttpStatusCode.TooManyRequests:
            logger.LogWarning(cosmosEx, "Rate-limited during {OperationDescription}. Retry after: {RetryAfter}.",
              operationDescription,
              cosmosEx.RetryAfter);
            throw cosmosEx;

          case System.Net.HttpStatusCode.NotFound:
            logger.LogError(cosmosEx, "Resource not found during {OperationDescription}.", operationDescription);
            throw cosmosEx;

          case System.Net.HttpStatusCode.BadRequest:
            logger.LogError(cosmosEx, "Bad request during {OperationDescription}. Message: {Message}",
              operationDescription,
              cosmosEx.Message);
            throw cosmosEx;

          default:
            logger.LogError(cosmosEx, "CosmosException during {OperationDescription}. StatusCode: {StatusCode}, Message: {Message}",
              operationDescription,
              cosmosEx.StatusCode,
              cosmosEx.Message);
            throw cosmosEx;
        }
      }
      else
      {
        logger.LogError(ex, "Unexpected error during {OperationDescription}.", operationDescription);
        throw new InvalidOperationException($"An unexpected error occurred during {operationDescription}.", ex);
      }
    }
  }
}

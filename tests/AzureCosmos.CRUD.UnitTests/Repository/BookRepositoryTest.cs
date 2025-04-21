using System.Net;
using AzureCosmos.CRUD.DataAccess.Config;
using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.UnitTests.Configs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AzureCosmos.CRUD.UnitTests.Repository
{
  public class BookRepositoryTest
  {
    private readonly ILogger<BookRepository> logger;
    private readonly IOptions<CosmosSettings> dbSettings;
    private readonly CosmosClient cosmosClient;
    private readonly BookRepository repository;

    public BookRepositoryTest()
    {
      logger = Substitute.For<ILogger<BookRepository>>();
      dbSettings = MockConfig.options;
      cosmosClient = Substitute.For<CosmosClient>();
      repository = new BookRepository(logger, dbSettings, cosmosClient);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnBook_WhenBookExists()
    {
      // Arrange  
      var bookId = "1";
      var expectedBook = new Book { Id = bookId, Title = "Test Book" };
      ItemResponse<Book> itemResponse = MockItemResponse(expectedBook, HttpStatusCode.OK);
      cosmosClient.GetContainer(dbSettings.Value.DatabaseName, dbSettings.Value.ContainerName)
        .ReadItemAsync<Book>(bookId, Arg.Any<PartitionKey>(), null, default)
        .Returns(itemResponse);

      // Act  
      var result = await repository.GetBookAsync(bookId);

      // Assert  
      Assert.NotNull(result);
      Assert.Equal(expectedBook.Id, result.Id);
      Assert.Equal(expectedBook.Title, result.Title);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNull_WhenBookDoesNotExist()
    {
      // Arrange  
      var bookId = "1";
      cosmosClient.GetContainer(dbSettings.Value.DatabaseName, dbSettings.Value.ContainerName)
        .ReadItemAsync<Book>(bookId, Arg.Any<PartitionKey>(), null, default)
        .ThrowsAsync(new CosmosException("Item not found", HttpStatusCode.NotFound, 404, "12345", 10.0));

      // Act  
      var result = await repository.GetBookAsync(bookId);

      // Assert  
      Assert.Null(result);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNull_WhenRequestIsBad()
    {
      // Arrange  
      var bookId = "1";
      cosmosClient.GetContainer(dbSettings.Value.DatabaseName, dbSettings.Value.ContainerName)
        .ReadItemAsync<Book>(bookId, Arg.Any<PartitionKey>(), null, default)
        .ThrowsAsync(new CosmosException("Item not found", HttpStatusCode.BadRequest, 400, "12345", 10.0));

      // Act  
      var result = await repository.GetBookAsync(bookId);

      // Assert  
      Assert.Null(result);
    }

    private static ItemResponse<T> MockItemResponse<T>(T expectedResponse, HttpStatusCode statusCode)
    {
      var itemResponse = Substitute.For<ItemResponse<T>>();
      itemResponse.Resource.Returns(expectedResponse);
      itemResponse.StatusCode.Returns(statusCode);

      return itemResponse;
    }
  }
}

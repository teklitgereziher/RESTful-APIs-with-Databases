using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace AzureCosmos.CRUD.IntegrationTests.Repository
{
  [Collection("IntegrationTestCollection")]
  public class BookRepositoryTest
  {
    private readonly IBookRepository repository;

    public BookRepositoryTest(IntegrationTestFactory factory)
    {
      repository = factory.Services.CreateScope().ServiceProvider.GetService<IBookRepository>();
    }

    [Fact]
    public async Task InsertOrReplaceAsync_ShouldReturnBook_WhenBookInserted()
    {
      // Arrange  
      var bookId = "1";
      var book = new Book { Id = bookId, Title = "Test Book" };

      // Act  
      var result = await repository.InsertOrReplaceAsync(book);

      // Assert  
      Assert.NotNull(result);
      Assert.Equal(book.Id, result.Id);
      Assert.Equal(book.Title, result.Title);
    }

    [Fact]
    public async Task GetBookById_ShouldReturnNull_WhenBookDoesNotExist()
    {
      // Arrange  
      var bookId = "2";

      // Act  
      var result = await repository.GetBookAsync(bookId);

      // Assert  
      Assert.Null(result);
    }

    [Fact]
    public async Task AddBookAsync_ShouldReturnBook_WhenBookIsAdded()
    {
      // Arrange
      var newBook = new Book { Id = "3", Title = "New Book" };

      // Act
      var result = await repository.AddBookAsync(newBook);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(newBook.Id, result.Id);
      Assert.Equal(newBook.Title, result.Title);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnTrue_WhenBookIsDeleted()
    {
      // Arrange
      var newBook = new Book { Id = "4", Title = "New Book" };
      var result = await repository.AddBookAsync(newBook);

      // Act
      var deleteResult = await repository.DeleteBookAsync(newBook.Id);

      // Assert
      Assert.NotNull(deleteResult);
      Assert.True(deleteResult);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnFalse_WhenBookDoesNotExist()
    {
      // Arrange
      var bookId = "5";

      // Act
      var result = await repository.DeleteBookAsync(bookId);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldReturnUpdatedBook_WhenBookIsUpdated()
    {
      // Arrange
      var newBook = new Book { Id = "6", Title = "New Book" };
      var result = await repository.AddBookAsync(newBook);

      var updatedTitle = "Updated Title";

      // Act
      var updatedBook = await repository.UpdateBookAsync(newBook.Id, updatedTitle);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(newBook.Id, updatedBook.Id);
      Assert.Equal(updatedTitle, updatedBook.Title);
    }

    [Fact]
    public async Task QueryBooksAsync_ShouldReturnBooks_WhenBooksMatchQuery()
    {
      // Arrange
      var bookTitle = "Test Book";
      var books = new List<Book>
        {
            new Book { Id = "7", Title = bookTitle },
            new Book { Id = "8", Title = bookTitle }
        };
      foreach (var book in books)
      {
        await repository.AddBookAsync(book);
      }

      // Act
      var result = await repository.QueryBooksAsync(bookTitle);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(books.Count, result.Count);
      Assert.All(result, book => Assert.Equal(bookTitle, book.Title));
    }

    //private static ItemResponse<T> MockItemResponse<T>(T expectedResponse, HttpStatusCode statusCode)
    //{
    //  var itemResponse = Substitute.For<ItemResponse<T>>();
    //  itemResponse.Resource.Returns(expectedResponse);
    //  itemResponse.StatusCode.Returns(statusCode);

    //  return itemResponse;
    //}
  }
}

using System.Net;
using System.Net.Http.Json;
using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.DataAccess.Repository;
using AzureCosmos.CRUD.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AzureCosmos.CRUD.IntegrationTests.RestApiTests
{
  [Collection("IntegrationTestCollection")]
  public class BooksControllerApiTest
  {
    private readonly IBookRepository bookRepository;
    private readonly HttpClient clientWithAuth;
    private readonly HttpClient clientWithoutAuth;

    public BooksControllerApiTest(IntegrationTestFactory factory)
    {
      clientWithAuth = factory.WithAuthentication()
        .CreateAndConfigureClient();
      clientWithoutAuth = factory.CreateClient();
      bookRepository = factory.Services.CreateScope().ServiceProvider.GetService<IBookRepository>();
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidBook_ReturnsCreatedAndPersistsBook()
    {
      // Arrange  
      Book newBook = CreateTestBook();

      // Act
      var response = await clientWithoutAuth.PostAsJsonAsync("/api/books/addbook", newBook);

      // Assert  
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var data = await response.Content.ReadAsStringAsync();
      var book = JsonConvert.DeserializeObject<Book>(data);

      var persistedBook = await bookRepository.GetBookAsync(newBook.Id);

    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidBook_ReturnsCreatedAndPersistsBook_2()
    {
      // Arrange  
      Book newBook = CreateTestBook();

      // Act
      var response = await clientWithoutAuth.PostAsJsonAsync("/api/books/addbook", newBook);

      // Assert  
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var data = await response.Content.ReadAsStringAsync();
      var book = JsonConvert.DeserializeObject<Book>(data);

      var response2 = await clientWithoutAuth.GetAsync($"/api/books/book?bookId={newBook.Id}");

      // Assert  
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var data2 = await response.Content.ReadAsStringAsync();
      var book2 = JsonConvert.DeserializeObject<Book>(data2);
      Assert.Equal(book.Id, book2.Id);
      Assert.Equal(book.Title, book2.Title);
    }

    private static Book CreateTestBook()
    {
      return new Book
      {
        Id = "123",
        Title = "Test Book",
        Year = 1989,
        Price = 10.9M,
        Author = new Author
        {
          Name = "Adam Smith",
          Phone = "+46 12345",
          Email = "example@example.com",
          Website = "example.com"
        },
        Publisher = new Publisher
        {
          Name = "McGill Publisher",
          Phone = "+33 59745949",
          Email = "mcgill@example.com",
          Website = "mcgill.com"
        }
      };
    }
  }
}

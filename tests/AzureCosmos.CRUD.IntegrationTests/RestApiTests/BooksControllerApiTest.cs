using System.Net;
using System.Net.Http.Json;
using System.Text;
using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.IntegrationTests.Setup;
using Newtonsoft.Json;

namespace AzureCosmos.CRUD.IntegrationTests.RestApiTests
{
  public class BooksControllerApiTest : ApiBaseIntegrationTest
  {
    private readonly IntegrationTestFactory factory;
    public BooksControllerApiTest(IntegrationTestFactory factory) : base(factory)
    {
      this.factory = factory;
    }

    [Fact]
    public async Task AddBook_WhenCalledWithValidBook_ReturnsCreatedAndPersistsBook()
    {
      // Arrange  
      var newBook = new Book
      {
        ISBN = "123",
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

      var requestContent = new StringContent(JsonConvert.SerializeObject(newBook), Encoding.UTF8, "application/json");

      // Act
      var response = await clientWithoutAuth.PostAsJsonAsync("/api/books/addbook", newBook);

      // Assert  
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var data = await response.Content.ReadAsStringAsync();
      var book = JsonConvert.DeserializeObject<Book>(data);

      var persistedBook = await bookRepository.GetBookAsync(newBook.ISBN);

    }
  }
}

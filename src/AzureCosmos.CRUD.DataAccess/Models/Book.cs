using Newtonsoft.Json;

namespace AzureCosmos.CRUD.DataAccess.Models
{
  public class Book
  {
    [JsonProperty(PropertyName = "bookId")]
    public string BookId { get; set; }
    [JsonProperty(PropertyName = "userId")]
    public string UserId { get; set; }
    [JsonProperty(PropertyName = "publsherId")]
    public string PublsherId { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }

    public Author Author { get; set; }
    //public Publisher Publisher { get; set; }
  }
}

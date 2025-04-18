using Newtonsoft.Json;

namespace AzureCosmos.CRUD.DataAccess.Models
{
  public class Book
  {
    [JsonProperty(PropertyName = "id")]
    public string ISBN { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }

    public Author Author { get; set; }
    public Publisher Publisher { get; set; }
  }
}

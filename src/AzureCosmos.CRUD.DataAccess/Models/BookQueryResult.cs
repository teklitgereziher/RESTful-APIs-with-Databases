namespace AzureCosmos.CRUD.DataAccess.Models
{
  public class BookQueryResult
  {
    public BookQueryResult(List<Book> books)
    {
      Books = books;
    }

    public List<Book> Books { get; set; } = [];
    public string ContinuationToken { get; set; } = string.Empty;
  }
}

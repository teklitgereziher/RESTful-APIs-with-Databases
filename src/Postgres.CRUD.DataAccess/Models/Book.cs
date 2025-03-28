namespace Postgres.CRUD.DataAccess.Models
{
  public class Book
  {
    public int Id { get; set; }
    public string Isbn { get; set; }
    public string Title { get; set; }

    public List<Author> Authors { get; set; }
  }
}

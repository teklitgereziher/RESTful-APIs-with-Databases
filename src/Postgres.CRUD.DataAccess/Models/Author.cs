namespace Postgres.CRUD.DataAccess.Models
{
  public class Author
  {
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public Address Address { get; set; }
  }
}

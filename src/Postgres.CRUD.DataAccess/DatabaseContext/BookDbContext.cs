using Microsoft.EntityFrameworkCore;
using Postgres.CRUD.DataAccess.Models;

namespace Postgres.CRUD.DataAccess.DatabaseContext
{
  public class BookDbContext : DbContext
  {
    public DbSet<Book> Books { get; set; }
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Book>().HasKey(b => b.Id);
      modelBuilder.Entity<Book>()
        .HasIndex(b => b.Isbn)
        .IsUnique();

      modelBuilder.Entity<Book>()
        .OwnsMany(b => b.Authors, a =>
        {
          a.ToJson();
          a.OwnsOne(a => a.Address);
        });
    }
  }
}

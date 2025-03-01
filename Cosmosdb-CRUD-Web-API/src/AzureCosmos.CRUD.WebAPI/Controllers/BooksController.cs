using System.ComponentModel.DataAnnotations;
using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Cosmos;

namespace AzureCosmos.CRUD.WebAPI.Controllers
{
  [ApiController]
  [Route("books")]
  public class BooksController : ControllerBase
  {
    private readonly ILogger<BooksController> logger;
    private readonly IBookRepository bookRepository;

    public BooksController(ILogger<BooksController> logger, IBookRepository bookRepository)
    {
      this.logger = logger;
      this.bookRepository = bookRepository;
    }

    [HttpGet]
    [Route("book")]
    public async Task<IActionResult> GetBookAsync([Required] string bookId)
    {
      try
      {
        var book = await bookRepository.GetBookAsync(bookId);
        if (book == null)
        {
          return NotFound();
        }

        return Ok(book);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while getting a book.");
        return StatusCode(500, "Internal server error");
      }
    }

    [HttpGet]
    [Route("list")]
    public async Task<IActionResult> GetBooksAsync([Required][FromQuery] List<string> bookIds)
    {
      try
      {
        var books = await bookRepository.GetBooksAsync([.. bookIds.Select(id => (id, new PartitionKey(id)))]);
        if (!books.Any())
        {
          return NotFound();
        }

        return Ok(books);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while getting books.");
        return StatusCode(500, "Internal server error");
      }
    }

    [HttpPost]
    [Route("addbook")]
    public async Task<IActionResult> AddBook([BindRequired][FromBody] Book book)
    {
      try
      {
        var result = await bookRepository.InsertOrReplaceAsync(book);

        return Ok(result);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while adding book to database.");
        return StatusCode(500, "Internal server error");
      }
    }
  }
}

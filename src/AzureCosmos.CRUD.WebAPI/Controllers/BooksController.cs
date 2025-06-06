using System.ComponentModel.DataAnnotations;
using AzureCosmos.CRUD.DataAccess.Models;
using AzureCosmos.CRUD.DataAccess.Repository;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Cosmos;
//using Microsoft.Identity.Web.Resource;

namespace AzureCosmos.CRUD.WebAPI.Controllers
{
  //[Authorize]
  //[RequiredScope(AcceptedScope = ["ToDoList.Read"])]
  //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
  //[RequiredScopeOrAppPermission(
  //  RequiredScopesConfigurationKey = "AzureAd:Scopes",
  //  RequiredAppPermissionsConfigurationKey = "AzureAd:AppPermissions"
  //)]
  [ApiController]
  [Route("api/books")]
  public class BooksController : ControllerBase
  {
    private const string InternalServerErrorMessage = "Internal server error";
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
        if (OperatingSystem.IsWindows())
        {
          logger.LogInformation("This is a Windows operating system.");
        }
        else if (OperatingSystem.IsLinux())
        {
          logger.LogInformation("This is a Linux operating system.");
        }
        else if (OperatingSystem.IsMacOS())
        {
          logger.LogInformation("This is a macOS operating system.");
        }
        else
        {
          logger.LogInformation("This is an unknown operating system.");
        }
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
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
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
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
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
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
      }
    }

    [HttpPost]
    public async Task<IActionResult> AddBulkBooks([Required][FromQuery] int numOfBooks)
    {
      try
      {
        await bookRepository.BulkInsertAsync(numOfBooks);

        return Ok();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while adding book to database.");
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
      }
    }

    [HttpPost]
    [Route("updatebook")]
    public async Task<IActionResult> UpdateBook([Required][FromQuery] string bookId, string bookTitle)
    {
      try
      {
        var result = await bookRepository.UpdateBookAsync(bookId, bookTitle);
        if (result == null)
        {
          return NotFound();
        }

        return Ok(result);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while updating book.");
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
      }
    }

    [HttpDelete]
    [Route("deletebook")]
    public async Task<IActionResult> DeleteBook([Required][FromQuery] string bookId)
    {
      try
      {
        var result = await bookRepository.DeleteBookAsync(bookId);
        if (result == true)
        {
          return Ok();
        }
        else if (result == false)
        {
          return Ok("Failed to delete the book.");
        }
        return NoContent();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error while deleting book.");
        return StatusCode(StatusCodes.Status500InternalServerError, InternalServerErrorMessage);
      }
    }
  }
}

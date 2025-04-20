using Microsoft.AspNetCore.Mvc;

namespace Postgres.CRUD.WebAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class BookController : ControllerBase
  {
    private static readonly string[] Summaries =
    [
      "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<BookController> logger;

    public BookController(ILogger<BookController> logger)
    {
      this.logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
      logger.LogInformation("GetWeatherForecast called");

      return Enumerable.Range(1, 5).Select(index => new WeatherForecast
      {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
      })
      .ToArray();
    }
  }
}

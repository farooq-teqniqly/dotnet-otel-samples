using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiSample.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private readonly ApplicationDbContext _dbContext;

    public WeatherForecastController(ApplicationDbContext dbContext)
    {
      ArgumentNullException.ThrowIfNull(dbContext);

      _dbContext = dbContext;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
      Activity.Current?.SetTag("user.is_authenticated", HttpContext.User.Identity?.IsAuthenticated);

      var summaries = await _dbContext
        .Summaries.Select(s => s.Value)
        .ToArrayAsync(HttpContext.RequestAborted)
        .ConfigureAwait(false);

      return
      [
        .. Enumerable
          .Range(1, 5)
          .Select(index => new WeatherForecast
          {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
            Summary = summaries[RandomNumberGenerator.GetInt32(summaries.Length)],
          }),
      ];
    }
  }
}

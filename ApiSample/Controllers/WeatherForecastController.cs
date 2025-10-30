using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace ApiSample.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private static readonly string[] _summaries =
    [
      "Freezing",
      "Bracing",
      "Chilly",
      "Cool",
      "Mild",
      "Warm",
      "Balmy",
      "Hot",
      "Sweltering",
      "Scorching",
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
      Activity.Current?.SetTag("user.is_authenticated", HttpContext.User.Identity?.IsAuthenticated);

      return
      [
        .. Enumerable
          .Range(1, 5)
          .Select(index => new WeatherForecast
          {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = RandomNumberGenerator.GetInt32(-20, 55),
            Summary = _summaries[RandomNumberGenerator.GetInt32(_summaries.Length)],
          }),
      ];
    }
  }
}

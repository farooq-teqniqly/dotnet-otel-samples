using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiSample.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private readonly IDbConnection _dbConnection;

    public WeatherForecastController(IDbConnection dbConnection)
    {
      ArgumentNullException.ThrowIfNull(dbConnection);

      _dbConnection = dbConnection;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
      var tier = GetTier();

      ApplicationDiagnostics.WeatherForecastRequestsCounter.Add(
        1,
        new KeyValuePair<string, object?>("user.membership", tier.ToString())
      );

      Activity.Current?.SetTag("user.is_authenticated", HttpContext.User.Identity?.IsAuthenticated);

      var sql = "SELECT value FROM dbo.summaries (NOLOCK);";

      var summaries = (await _dbConnection.QueryAsync<string>(sql).ConfigureAwait(false)).ToArray();

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

    private static Tier GetTier() =>
      RandomNumberGenerator.GetInt32(0, 2) == 0 ? Tier.Standard : Tier.Premium;
  }

  internal enum Tier
  {
    Standard,
    Premium,
  }
}

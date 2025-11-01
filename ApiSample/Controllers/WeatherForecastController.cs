using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;

namespace ApiSample.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(
      IDbConnection dbConnection,
      ILogger<WeatherForecastController> logger
    )
    {
      ArgumentNullException.ThrowIfNull(dbConnection);
      ArgumentNullException.ThrowIfNull(logger);

      _dbConnection = dbConnection;
      _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
      var tier = GetTier();

      _logger.LogDebug("Incoming request for tier {Tier}", tier.ToString());

      Baggage.SetBaggage("user.membership", tier.ToString());

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

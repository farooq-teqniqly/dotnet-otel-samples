using System.Diagnostics.Metrics;

namespace ApiSample
{
  internal static class ApplicationDiagnostics
  {
    public const string ServiceName = "SampleApi";
    public const string ServiceNamespace = "Teqniqly";
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<long> WeatherForecastRequestsCounter = Meter.CreateCounter<long>(
      "weather_forecast_requests_total"
    );
  }
}

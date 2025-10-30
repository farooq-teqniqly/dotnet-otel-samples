using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApiSample
{
  internal static class DependencyInjectionExtensions
  {
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
      builder
        .Services.AddOpenTelemetry()
        .ConfigureResource(r =>
          r.AddService(
            "ApiSample",
            "Teqniqly",
            Assembly.GetExecutingAssembly().GetName().Version!.ToString()
          )
        )
        .WithTracing(t =>
          t.AddAspNetCoreInstrumentation(opts => opts.Filter = null)
            .AddConsoleExporter()
            .AddOtlpExporter(builder.Configuration)
        );

      return builder;
    }

    public static TracerProviderBuilder AddOtlpExporter(
      this TracerProviderBuilder builder,
      IConfiguration configuration
    )
    {
      var otelEndpoint = configuration["Otel:Endpoint"];

      if (string.IsNullOrWhiteSpace(otelEndpoint))
      {
        return builder;
      }

      builder.AddOtlpExporter(opts =>
      {
        opts.Endpoint = new Uri(otelEndpoint);
      });

      return builder;
    }
  }
}

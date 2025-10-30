using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ConsoleSample
{
  internal static class ApplicationDiagnostics
  {
    internal const string ActivitySourceName = "ConsoleSample.Diagnostics";
    internal const string ServiceName = "ConsoleSample";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);
  }

  internal static class Program
  {
    private static async Task DoWork()
    {
      await StepOne();
      await StepTwo();
    }

    static async Task Main()
    {
      using (
        var tracerProvider = OpenTelemetry
          .Sdk.CreateTracerProviderBuilder()
          .SetResourceBuilder(
            ResourceBuilder.CreateDefault().AddService(ApplicationDiagnostics.ServiceName)
          )
          .AddSource(ApplicationDiagnostics.ActivitySourceName)
          .AddConsoleExporter()
          .Build()
      )
      {
        using (var activity = ApplicationDiagnostics.ActivitySource.StartActivity("DoWork"))
        {
          await DoWork();
          Console.WriteLine("Done!");
        }
      }
    }

    private static async Task StepOne()
    {
      await Task.Delay(500);
    }

    private static async Task StepTwo()
    {
      await Task.Delay(1000);
    }
  }
}

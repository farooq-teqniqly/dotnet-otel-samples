using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApiSample
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddControllers();

      builder.Services.AddOpenApi();

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
          t.AddAspNetCoreInstrumentation(opts => opts.Filter = null).AddConsoleExporter()
        );

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      if (app.Environment.IsDevelopment())
      {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();

      app.MapControllers();

      app.Run();
    }
  }
}

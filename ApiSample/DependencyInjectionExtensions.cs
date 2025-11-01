using System.Data;
using System.Reflection;
using ApiSample.Entities;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApiSample
{
  internal static class DependencyInjectionExtensions
  {
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
      var connectionString =
        builder.Configuration.GetConnectionString("WeatherDb")
        ?? throw new InvalidOperationException("Database connection string not configured");

      builder.Services.AddDbContext<ApplicationDbContext>(opts =>
      {
        opts.UseSqlServer(connectionString).UseSnakeCaseNamingConvention();

        if (builder.Environment.IsDevelopment())
        {
          opts.EnableSensitiveDataLogging();
        }
      });

      builder.AddDapper();

      return builder;
    }

    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
      builder
        .Services.AddOpenTelemetry()
        .ConfigureResource(r =>
          r.AddService(
            ApplicationDiagnostics.ServiceName,
            ApplicationDiagnostics.ServiceNamespace,
            Assembly.GetExecutingAssembly().GetName().Version!.ToString()
          )
        )
        .WithTracing(t =>
          t.AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSqlClientInstrumentation()
            .AddOtlpExporter(builder.Configuration)
        )
        .WithMetrics(m =>
          m.AddMeter(ApplicationDiagnostics.Meter.Name)
            .AddConsoleExporter()
            .AddPrometheusExporter()
            .AddAspNetCoreInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
            .AddSqlClientInstrumentation()
        );

      return builder;
    }

    public static async Task ApplyMigrationsAsync(
      this WebApplication app,
      CancellationToken cancellationToken = default
    )
    {
      ArgumentNullException.ThrowIfNull(app);

      using (var scope = app.Services.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using (dbContext)
        {
          try
          {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            app.Logger.LogInformation("Successfully migrated database");
          }
#pragma warning disable S2139
          catch (Exception ex)
          {
            app.Logger.LogError(ex, "Database migration failed");
            throw;
          }
#pragma warning restore S2139
        }
      }
    }

    public static async Task SeedDatabaseAsync(
      this WebApplication app,
      CancellationToken cancellationToken = default
    )
    {
      ArgumentNullException.ThrowIfNull(app);

      string[] summaries =
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

      using (var scope = app.Services.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using (dbContext)
        {
          try
          {
            var dbSummaries = await dbContext
              .Summaries.Select(s => s.Value)
              .ToArrayAsync(cancellationToken)
              .ConfigureAwait(false);

            var summariesToInsert = summaries.Except(dbSummaries);

            await dbContext
              .Summaries.AddRangeAsync(
                summariesToInsert.Select(val => new Summary { Value = val }),
                cancellationToken
              )
              .ConfigureAwait(false);

            var summarySeededCount = await dbContext
              .SaveChangesAsync(cancellationToken)
              .ConfigureAwait(false);

            app.Logger.LogInformation(
              "Seeded {SummarySeededCount} summaries into database",
              summarySeededCount
            );
          }
#pragma warning disable S2139
          catch (Exception exception)
          {
            app.Logger.LogError(exception, "Database seeding failed");
            throw;
          }
#pragma warning restore S2139
        }
      }
    }

    private static WebApplicationBuilder AddDapper(this WebApplicationBuilder builder)
    {
      builder.Services.AddScoped<IDbConnection>(sp =>
      {
        var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
          connection.Open();
        }

        return connection;
      });

      return builder;
    }

    private static TracerProviderBuilder AddOtlpExporter(
      this TracerProviderBuilder builder,
      ConfigurationManager configuration
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

namespace ApiSample
{
  internal static class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddControllers();
      builder.Services.AddOpenApi();
      builder.AddOpenTelemetry();
      builder.AddDatabase();
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      if (app.Environment.IsDevelopment())
      {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();

        var cancellationToken = app.Lifetime.ApplicationStopping;

        await app.ApplyMigrationsAsync(cancellationToken).ConfigureAwait(false);
        await app.SeedDatabaseAsync(cancellationToken).ConfigureAwait(false);
      }

      app.UseHttpsRedirection();
      app.UseCors();
      app.UseAuthorization();

      app.MapControllers();

      await app.RunAsync().ConfigureAwait(false);
    }
  }
}

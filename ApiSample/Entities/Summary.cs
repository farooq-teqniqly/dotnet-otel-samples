namespace ApiSample.Entities
{
  public sealed class Summary
  {
    public Guid Id { get; set; }
    public required string Value { get; init; }
  }
}

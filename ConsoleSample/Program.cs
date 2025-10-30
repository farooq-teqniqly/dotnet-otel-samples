namespace ConsoleSample
{
  internal static class Program
  {
    private static async Task DoWork()
    {
      await StepOne();
      await StepTwo();
    }

    static async Task Main()
    {
      await DoWork();
      Console.WriteLine("Done!");
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

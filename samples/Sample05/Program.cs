namespace Sample05;

internal sealed class Program
{
    static async Task Main(string[] args)
    {
        await new App().RunAsync(args).ConfigureAwait(false);
    }
}

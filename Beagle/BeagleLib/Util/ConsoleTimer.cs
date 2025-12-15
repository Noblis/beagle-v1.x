using System.Diagnostics;

namespace BeagleLib.Util;

public class ConsoleTimer : IDisposable
{
    public ConsoleTimer(string name, bool enabled = true, ConsoleColor? color = null)
    {
        Enabled = enabled;
        if (!Enabled) return;

        DefaultColor = Console.ForegroundColor;
        if (color != null) Console.ForegroundColor = color.Value;
        //if (Environment.OSVersion.Platform == PlatformID.Unix) DefaultColor = ConsoleColor.DarkBlue;

        Output.Write($"{name}...");
        Watch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (!Enabled) return;

        Watch!.Stop();
        Output.Write("\b\b Done in ");

        Console.ForegroundColor = ConsoleColor.Red;
        Output.WriteLine($"{Watch.Elapsed:c}.");
        Console.ForegroundColor = DefaultColor;
    }

    
    public Stopwatch? Watch { get; set; }
    
    private ConsoleColor DefaultColor { get; set; }
    private bool Enabled { get; }
}
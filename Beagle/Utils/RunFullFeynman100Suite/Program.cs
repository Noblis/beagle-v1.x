using System.Diagnostics;
using System.Text;

namespace RunFullFeynman100Suite;

public static class Program
{
    static void Main()
    {
        const int stopAfterMin = 10;
        const int feynmanEqCount = 100;
        const int numberOfRunsPerEq = 10;
        
        DateTime now = DateTime.Now;
        
        var equationsValidatedCount = new int[feynmanEqCount];
        Environment.CurrentDirectory = "..\\..\\..\\..\\..\\Run";
        if (Directory.Exists("AppOutput")) Directory.Delete("AppOutput", true);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            CreateNoWindow = false,
            UseShellExecute = false
        };

        try
        {
            for (var eq = 1; eq <= feynmanEqCount; eq++)
            {
                startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={stopAfterMin} RunFeynman={eq} NoEscMenu #useLibDevice";
                for (var i = 1; i <= numberOfRunsPerEq; i++)
                {
                    using (var process = Process.Start(startInfo) ?? throw new Exception())
                    {
                        process.WaitForExit();
                        var exitCode = process.ExitCode;
                        if (exitCode == 0)
                        {
                            equationsValidatedCount[eq-1]++;
                        }
                        else if (exitCode != 1)
                        {
                            Console.WriteLine($"Beagle crashed while executing equation {eq}");
                            Environment.Exit(exitCode);
                        }
                        
                        //print and save results
                        var resultsSb = new StringBuilder();
                        for (var eqi = 1; eqi <= eq - 1; eqi++)
                        {
                            resultsSb.AppendLine($"Equation {eqi}: {equationsValidatedCount[eqi-1]}/{numberOfRunsPerEq}");
                        }
                        resultsSb.AppendLine($"Equation {eq}: {equationsValidatedCount[eq-1]}/{i}");
                        resultsSb.AppendLine("------------------------------------------------------------------");
                        resultsSb.AppendLine($"Typical Solved: {equationsValidatedCount.Count(x => x >= MathF.Ceiling(numberOfRunsPerEq/2f))}/{eq}");
                        resultsSb.AppendLine($"Best Solved: {equationsValidatedCount.Count(x => x > 0)}/{eq}");

                        var results = resultsSb.ToString();

                        var fgColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write(results);
                        Console.ForegroundColor = fgColor;

                        File.WriteAllText("AppOutput/results.txt", results);

                        Console.WriteLine("Waiting for 5 seconds before starting next...");
                        Thread.Sleep(5_000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }

        Directory.Move("AppOutput", $"FeynmanAppOutput_{now.Year}-{now.Month:D2}-{now.Day:D2}-{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}");
    }
}
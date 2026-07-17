using System.Diagnostics;

namespace RunFullFeynman100Suite;

public static class Program
{
    static void Main()
    {
        DateTime now = DateTime.Now;
        
        var equationsValidatedCount = new int[100];
        Environment.CurrentDirectory = "..\\..\\..\\..\\..\\Run";
        Directory.Delete("AppOutput", true);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            CreateNoWindow = false,
            UseShellExecute = false
        };

        try
        {
            for (var eq = 1; eq <= 100; eq++)
            {
                startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin=1 RunFeynman={eq} NoEscMenu #useLibDevice";
                for (var i = 0; i < 10; i++)
                {
                    using (var process = Process.Start(startInfo) ?? throw new Exception())
                    {
                        process.WaitForExit();
                        var exitCode = process.ExitCode;
                        if (exitCode == 0)
                        {
                            equationsValidatedCount[eq]++;
                        }
                        else if (exitCode != 1)
                        {
                            Console.WriteLine($"Beagle crashed while executing equation {eq}");
                            Environment.Exit(exitCode);
                        }
                        
                        //print results
                        for (var eqi = 1; eqi <= eq-1; eqi++)
                        {
                            Console.WriteLine($"Equation {eqi}: {equationsValidatedCount[eqi]}/10");
                        }
                        Console.WriteLine($"Equation {eq}: {equationsValidatedCount[eq]}/{i}");
                        Console.WriteLine("Waiting for 10 seconds before starting next...");
                        Thread.Sleep(10_000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start process: {ex.Message}");
        }

        Directory.Move("AppOutput", $"FeynmanAppOutput-_{now.Year}-{now.Month:D2}-{now.Day:D2}-{now.Hour:D2}-{now.Minute:D2}-{now.Second:D2}");
    }
}
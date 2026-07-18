using System.Diagnostics;
using System.Text;

namespace RunFullFeynman100Suite;

public static class Program
{
    private const int StopAfterMin = 10;
    private const int FeynmanEqCount = 100;
    private const int NumberOfRunsPerEq = 10;
    private const bool StopAfterTypicalAchievedStraight = true;

    private static void Main()
    {
        //const int stopAfterMin = 1;
        //const int feynmanEqCount = 3;
        //const int numberOfRunsPerEq = 3;
        //const bool stopAfterTypicalAchievedStraight = true;


        DateTime now = DateTime.Now;
        
        var equationsValidatedCount = new int[FeynmanEqCount];
        var equationsRanCount = new int[FeynmanEqCount];
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
            for (var eq = 1; eq <= FeynmanEqCount; eq++)
            {
                startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={StopAfterMin} RunFeynman={eq} NoEscMenu #useLibDevice";
                for (var i = 1; i <= NumberOfRunsPerEq; i++)
                {
                    using (var process = Process.Start(startInfo) ?? throw new Exception())
                    {
                        process.WaitForExit();
                        var exitCode = process.ExitCode;
                        
                        equationsRanCount[eq - 1]++;
                        if (exitCode == 0)
                        {
                            equationsValidatedCount[eq - 1]++;
                            // ReSharper disable RedundantLogicalConditionalExpressionOperand
                            if (equationsValidatedCount[eq - 1] >= MathF.Ceiling(NumberOfRunsPerEq / 2f) &&
                                equationsValidatedCount[eq - 1] == equationsRanCount[eq - 1] && 
                                StopAfterTypicalAchievedStraight)
                            {
                                GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount);
                                Console.WriteLine($"Typical is achieved straight ({equationsValidatedCount[eq - 1]}/{equationsRanCount[eq - 1]}), skipping the remaining runs...");
                                break;
                            }
                            // ReSharper restore RedundantLogicalConditionalExpressionOperand
                        }
                        else if (exitCode != 1)
                        {
                            Console.WriteLine($"Beagle crashed while executing equation {eq}");
                            Environment.Exit(exitCode);
                        }
                        
                        GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount);
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

    private static void GenerateAndDisplayResults(int[] equationsValidatedCount, int[] equationsRanCount)
    {
        var resultsSb = new StringBuilder();
        for (var eqi = 1; eqi <= FeynmanEqCount; eqi++)
        {
            resultsSb.AppendLine($"Equation {eqi}: {equationsValidatedCount[eqi-1]}/{equationsRanCount[eqi - 1]}");
        }

        var eqRan = equationsRanCount.Count(x => x > 0);
        resultsSb.AppendLine("----------------------");
        resultsSb.AppendLine($"Typical Solved: {equationsValidatedCount.Count(x => x >= MathF.Ceiling(NumberOfRunsPerEq/2f))}/{eqRan}");
        resultsSb.AppendLine($"Best Solved: {equationsValidatedCount.Count(x => x > 0)}/{eqRan}");

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
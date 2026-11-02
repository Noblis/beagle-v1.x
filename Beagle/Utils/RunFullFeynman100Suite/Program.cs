using System.Diagnostics;
using System.Text;

namespace RunFullFeynman100Suite;

public static class Program
{
    private enum ProblemType
    {
        Easy, Medium, Hard 
    }

    private static bool _runWithNoEscMenu = OperatingSystem.IsLinux();

    private const string RelativePathToRunProject = "../../../../../Run"; //if running from Visual Studio or Rider
    //private const string RelativePathToRunProject = "../../Run"; //if running from command line

    //private static readonly int[] _problems = [44, 90];
    private static readonly int[] _problems = Enumerable.Range(1, FeynmanEqCount).ToArray();


    private const int HardStopAfterMin = 10; 
    private const int MediumStopAfterMin = 4;
    private const int EasyStopAfterMin = 1;

    private const int FeynmanEqCount = 100;
    private const int NumberOfRunsPerEq = 10;

    // ReSharper disable once InconsistentNaming
    private static readonly int[] _easyProblems = Enumerable.Range(1, FeynmanEqCount).ToArray();
    private static readonly int[] _hardProblems = [];

    //private static readonly int[] _easyProblems = [1, 8, 10, 11, 12, 15, 16, 22, 25, 27, 28, 34, 37, 39, 40, 41, 42, 45, 46, 49, 53, 54, 55, 58, 59, 60, 66, 69, 70, 73, 74, 75, 76, 77, 78, 83, 84, 85, 88, 89, 92, 93, 96, 97, 100];
    //private static readonly int[] _hardProblems = [2, 3, 4, 5, 6, 7, 14, 17, 18, 19, 20, 21, 26, 29, 30, 31, 33, 36, 38, 43, 44, 50, 56, 57, 64, 67, 68, 72, 80, 86, 87, 90, 91, 95];
    //medium problems are all others

    private static void Main()
    {
        //Here is the logic for each formula
        //1) If problem is in difficult problems, start with full timing at step 4
        //2) If we get typical by: half successes in exactly half runs using express timing, we are done
        //3) If we get a failure while doing #1, we start over using full timing
        //4) As soon as we get half successes, we are done
        //5) As soon as we have at least one solution (for best) and can no longer achieve typical anymore, we are done 

        DateTime startTime = DateTime.Now;
        
        var equationsValidatedCount = new int[FeynmanEqCount];
        var equationsRanCount = new int[FeynmanEqCount];
        var ranAs = new ProblemType?[FeynmanEqCount];

        Environment.CurrentDirectory = RelativePathToRunProject;

        if (Directory.Exists("AppOutput")) Directory.Delete("AppOutput", true);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            CreateNoWindow = false,
            UseShellExecute = false,
        };

        try
        {
            foreach (var eq in _problems)
            {
                ProblemType runningAs;
                if (_hardProblems.Contains(eq))
                {
                    runningAs = ProblemType.Hard;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={HardStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }
                else if (_easyProblems.Contains(eq))
                {
                    runningAs = ProblemType.Easy;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={EasyStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }
                else
                {
                    runningAs = ProblemType.Medium;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={MediumStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }

                for (var i = 1; i <= NumberOfRunsPerEq; i++)
                {
                    var fgColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine();
                    Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");
                    Console.WriteLine();
                    Console.ForegroundColor = fgColor;
                    using (var process = Process.Start(startInfo) ?? throw new Exception())
                    {
                        process.WaitForExit();
                        var exitCode = process.ExitCode;
                        
                        equationsRanCount[eq - 1]++;
                        if (exitCode == 0)
                        {
                            ranAs[eq - 1] = runningAs;
                            equationsValidatedCount[eq - 1]++;
                            if (equationsValidatedCount[eq - 1] >= MathF.Ceiling(NumberOfRunsPerEq / 2f))
                                //&& equationsValidatedCount[eq - 1] == equationsRanCount[eq - 1])
                            {
                                GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount, ranAs, startTime, DateTime.Now);
                                Console.WriteLine($"Typical is achieved ({equationsValidatedCount[eq - 1]}/{equationsRanCount[eq - 1]}), skipping the remaining runs...");
                                break;
                            }
                        }
                        else if (exitCode == 1)
                        {
                            if (runningAs == ProblemType.Easy)
                            {
                                runningAs = ProblemType.Medium;
                                equationsRanCount[eq - 1] = equationsValidatedCount[eq - 1] = 0;
                                startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={MediumStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                                i = 0;
                            }
                            else if (runningAs == ProblemType.Medium)
                            {
                                runningAs = ProblemType.Hard;
                                equationsRanCount[eq - 1] = equationsValidatedCount[eq - 1] = 0;
                                startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={HardStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                                i = 0;
                            }
                            else
                            {
                                //if running as hard no hope for typical, but we already have best
                                if (equationsValidatedCount[eq - 1] > 0 &&
                                    NumberOfRunsPerEq - equationsRanCount[eq - 1] <
                                    MathF.Ceiling(NumberOfRunsPerEq / 2f) - equationsValidatedCount[eq - 1])
                                {
                                    ranAs[eq - 1] = runningAs;
                                    GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount, ranAs, startTime, DateTime.Now);
                                    Console.WriteLine($"Best is achieved, typical out of reach ({equationsValidatedCount[eq - 1]}/{equationsRanCount[eq - 1]}), skipping the remaining runs...");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Beagle crashed while executing equation {eq}. Retrying...");
                            equationsRanCount[eq - 1]--;
                            i--;
                            continue;
                        }

                        ranAs[eq - 1] = runningAs;
                        GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount, ranAs, startTime, DateTime.Now);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }

        while (true)
        {
            try
            {
                Directory.Move("AppOutput", $"FeynmanAppOutput_{startTime.Year}-{startTime.Month:D2}-{startTime.Day:D2}-{startTime.Hour:D2}-{startTime.Minute:D2}-{startTime.Second:D2}");
                break;
            }
            catch (Exception)
            {
                var fgColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("The AppOutput directory appears to be locked or in use by another application.");
                Console.WriteLine("Please close any applications using it and press Enter to retry...");
                Console.ReadLine();
                Console.ForegroundColor = fgColor;
            }
        }
    }

    private static void GenerateAndDisplayResults(int[] equationsValidatedCount, int[] equationsRanCount, ProblemType?[] wasSolvingAs, DateTime startTime, DateTime completionTime)
    {
        var resultsSb = new StringBuilder();
        for (var eqi = 1; eqi <= FeynmanEqCount; eqi++)
        {
            if (wasSolvingAs[eqi - 1] != null)
            {
                resultsSb.AppendLine($"Equation {eqi}: {equationsValidatedCount[eqi - 1]}/{equationsRanCount[eqi - 1]} - as {wasSolvingAs[eqi - 1]} problem");
            }
            else
            {
                resultsSb.AppendLine($"Equation {eqi}: {equationsValidatedCount[eqi - 1]}/{equationsRanCount[eqi - 1]}");
            }
        }

        var eqRan = equationsRanCount.Count(x => x > 0);
        resultsSb.AppendLine("----------------------");
        resultsSb.AppendLine($"Typical Solved: {equationsValidatedCount.Count(x => x >= MathF.Ceiling(NumberOfRunsPerEq/2f))}/{eqRan}");
        resultsSb.AppendLine($"Best Solved: {equationsValidatedCount.Count(x => x > 0)}/{eqRan}");
        resultsSb.AppendLine();
        resultsSb.AppendLine($"Started: {startTime}");
        resultsSb.AppendLine($"Completed: {completionTime}");
        resultsSb.AppendLine($"Took: {completionTime - startTime}");


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
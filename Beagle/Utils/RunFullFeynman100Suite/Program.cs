using System.Diagnostics;
using System.Text;

namespace RunFullFeynman100Suite;

public static class Program
{
    private enum ProblemType
    {
        Easy, Medium, Hard, VeryHard
    }

    private static readonly bool _runWithNoEscMenu = OperatingSystem.IsLinux();

    private const string RelativePathToRunProject = "../../../../../Run"; //if running from Visual Studio or Rider
    //private const string RelativePathToRunProject = "../../Run"; //if running from command line

    //private static readonly int[] _problems = [1, 2, 5, 44, 87, 90];
    private static readonly int[] _problems = [5,44,90,91];//Enumerable.Range(1, FeynmanEqCount).ToArray();


    //Easy and hard will only accept 5/5 wile hard and very hard will try to run x/10
    private const int EasyStopAfterMin = 1;
    private const int MediumStopAfterMin = 4;
    private const int HardStopAfterMin = 10;
    private const int VeryHardStopAfterMin = 30;//30;


    private const int FeynmanEqCount = 100;
    private const int NumberOfRunsPerEq = 10;

    // ReSharper disable once InconsistentNaming
    //If a ProblemType is listed in multiple arrays, very hard takes precedence over hard, etc. (veryHard > hard > medium).
    //Anything not listed is assumed easy at start
    private static readonly int[] _veryHardProblems = [];
    private static readonly int[] _hardProblems = [];
    private static readonly int[] _mediumProblems = [];

    private static void Main()
    {
        //Here is the logic 
        //- For easy and medium problems
        //      - We will only accept 5/5
        //      - If 5/5 is achieved, we are done with this problem otherwise the problem is upgraded to a higher category
        //- If a problem is considered hard or very hard:
        //      - As soon as we get half successes, we are done with the problem
        //      - As soon as we have at least one solution (for best) and can no longer achieve typical anymore, we are done with the problem
        //      - We upgrade from hard to very hard only if we could not solve a problem as hard even once (can't solve at all!) 

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
                if (_veryHardProblems.Contains(eq))
                {
                    runningAs = ProblemType.VeryHard;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={VeryHardStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }
                else if (_hardProblems.Contains(eq))
                {
                    runningAs = ProblemType.Hard;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={HardStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }
                else if (_mediumProblems.Contains(eq))
                {
                    runningAs = ProblemType.Medium;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={MediumStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                }
                else
                {
                    runningAs = ProblemType.Easy;
                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={EasyStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
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
                        if (exitCode == 0) //solved
                        {
                            ranAs[eq - 1] = runningAs;
                            equationsValidatedCount[eq - 1]++;
                            if (equationsValidatedCount[eq - 1] >= MathF.Ceiling(NumberOfRunsPerEq / 2f))
                            {
                                GenerateAndDisplayResults(equationsValidatedCount, equationsRanCount, ranAs, startTime, DateTime.Now);
                                Console.WriteLine($"Typical is achieved ({equationsValidatedCount[eq - 1]}/{equationsRanCount[eq - 1]}), skipping the remaining runs...");
                                break;
                            }

                            //if running as hard or very hard with no hope for typical, but we already have best
                            //the outer if statement is not even needed, we only get here if the problem is hard or very hard but 
                            //I kept it for clarity of logic
                            if (runningAs is ProblemType.Hard or ProblemType.VeryHard)
                            {
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
                        else if (exitCode == 1) //unsolved
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
                            else if (runningAs == ProblemType.Hard)
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

                                //if unsolved on last iteration
                                if (i == NumberOfRunsPerEq)
                                {
                                    runningAs = ProblemType.VeryHard;
                                    equationsRanCount[eq - 1] = equationsValidatedCount[eq - 1] = 0;
                                    startInfo.Arguments = $"run --configuration Release --no-launch-profile -- StopAfterMin={VeryHardStopAfterMin} RunFeynman={eq} {(_runWithNoEscMenu ? "NoEscMenu" : "")} #useLibDevice";
                                    i = 0;
                                }
                            }
                            else //problem is very hard 
                            {
                                //if running as hard with no hope for typical, but we already have best
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
        foreach (var eqi in _problems)
        {
            if (wasSolvingAs[eqi - 1] != null)
            {
                resultsSb.Append($"Equation {eqi}: {equationsValidatedCount[eqi - 1]}/{equationsRanCount[eqi - 1]} - as {wasSolvingAs[eqi - 1]} problem");
                if (equationsValidatedCount[eqi - 1] == 0) resultsSb.AppendLine(" <--- NOT SOLVED!");
                else resultsSb.AppendLine();
            }
            else
            {
                resultsSb.AppendLine($"Equation {eqi}: {equationsValidatedCount[eqi - 1]}/{equationsRanCount[eqi - 1]}");
            }
        }

        var eqRan = equationsRanCount.Count(x => x > 0);
        resultsSb.AppendLine("----------------------");
        resultsSb.AppendLine($"Typical Solved: {equationsValidatedCount.Count(x => x >= MathF.Ceiling(NumberOfRunsPerEq / 2f))}/{eqRan}");
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
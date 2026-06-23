using System;
using System.Diagnostics;
using System.IO;
#pragma warning disable CS0162 // Unreachable code detected

namespace Supermodel.Tooling.SolutionMaker.Cmd;

class Program
{
    static void Main()
    {
        try
        {
            SolutionMaker.Version = "9.5.0";

            //*******Un-comment and run this once to refresh the solution zip
            //Console.WriteLine($"v{SolutionMaker.Version}");
            //Console.WriteLine();

            //Console.Write("Deleting XXYXX\\Frameworks directory... ");
            //Directory.Delete(@"..\..\..\..\..\..\..\XXYXX.Core\XXYXX\Frameworks", true);
            //Console.WriteLine("Done!");

            //Console.Write("Copying Frameworks directory from TDM.Core to XXYXX... ");
            //CopyDirectory(@"..\..\..\..\..\..\Frameworks", @"..\..\..\..\..\..\..\XXYXX.Core\XXYXX\Frameworks");
            //Console.WriteLine("Done!");

            ////Adjust versions in XXYXX and in TDM
            //File.WriteAllText(SolutionMaker.CombineAndAdjustPaths(@"..\..\..\..\..\..\..\XXYXX.Core\XXYXX\", @"Frameworks\Version.txt"), $"Version {SolutionMaker.Version}");
            //File.WriteAllText(SolutionMaker.CombineAndAdjustPaths(@"..\..\..\..\..\..\", @"Frameworks\Version.txt"), $"Version {SolutionMaker.Version}");
            //Console.WriteLine("Version.txt files updated successfully!");

            //SolutionMaker.CreateSnapshot(@"..\..\..\..\..\..\..\XXYXX.Core\XXYXX", @"..\..\..\");
            //Console.WriteLine($"{SolutionMaker.ZipFileName} created successfully!");

            //return;
            //********Un-comment and run this once to refresh the solution zip
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();

            Console.WriteLine("Supermodel.Core Solution Maker");
            Console.WriteLine($"Version {SolutionMaker.Version}");

            //Comment this out for debugging, this is to speed up development and incremental testing
            Console.WriteLine("Please Enter Solution Parameters");
            var solutionMakerParams = SolutionMakerParams.ReadFromConsole();
            //End of comment out for debugging

            //Comment this out for production, this is to speed up development and incremental testing
            //var solutionMakerParams = new SolutionMakerParams
            //{
            //    SolutionName = "XYX",
            //    SolutionDirectory = @"C:\Users\ilyab\Documents\Projects",
            //    WebFramework = WebFrameworkEnum.WebMonk,
            //    Database = DatabaseEnum.SqlServer
            //};
            //End of comment out for production

            var path = solutionMakerParams.CalculateFullPath();
            if (Directory.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Directory ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(path);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(" already exists.\nWould you like to replace it? (y/n): ");
                    
                var input = Console.ReadLine()!;
                //if (input == null) return;
                input = input.Trim().ToLower();
                if (input != "y") return;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Deleting {path}...");
                Directory.Delete(SolutionMaker.CombineAndAdjustPaths(solutionMakerParams.SolutionDirectory, solutionMakerParams.SolutionName), true);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Generating new solution {solutionMakerParams.SolutionName}...");
            SolutionMaker.CreateSupermodelShell(solutionMakerParams);
                
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Solution {solutionMakerParams.SolutionName} generated successfully! Open it now? (y/n): ");
            var startSolution = Console.ReadLine()!;
            if (/*startSolution != null &&*/ startSolution.Trim().ToLower() == "y") 
            {
                new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = true, 
                        FileName = SolutionMaker.CombineAndAdjustPaths(path, $"{solutionMakerParams.SolutionName}.sln"),
                    }
                }.Start();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
        finally
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private static void CopyDirectory(string sourceDirName, string destDirName)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

        var dirs = dir.GetDirectories();
        
        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);        

        // Get the files in the directory and copy them to the new location.
        var files = dir.GetFiles();
        foreach (var file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        // Copy subdirectories and their contents to new location.
        foreach (var subDir in dirs)
        {
            string tempPath = Path.Combine(destDirName, subDir.Name);
            CopyDirectory(subDir.FullName, tempPath);
        }
    }
}
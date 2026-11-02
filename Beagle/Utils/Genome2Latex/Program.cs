using BeagleLib.Agent;
using BeagleLib.MathStackLib;
using BeagleLib.VM;
using Newtonsoft.Json;

namespace Genome2Latex;

public static class Program
{
    static void Main()
    {
        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CommandConverter());
            return settings;
        };

        string[]? inputLabels = null;

        Console.WriteLine("Welcome to Genome Json to LaTeX converter!");
        while (true)
        {
            Console.WriteLine("Please enter a genome or 'q' for quit:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var json = Console.ReadLine()!;
            Console.ResetColor();
            if (json.Trim().ToLower() == "q") break;            
            
            var organism = Organism.CreateFromAnyString(json);
            var maxInputIdx = organism.Commands.GetMaxInputIdx();

            if (inputLabels == null || inputLabels.Length != maxInputIdx + 1)
            {
                inputLabels = new string[maxInputIdx + 1];
                ReadInputLabelsFromUser(inputLabels);
            }
            else
            {
                Console.Write("Would you like to reuse input labels from last run (y/n)?");
                var input = Console.ReadLine();
                if (input != "y") ReadInputLabelsFromUser(inputLabels);
            }
            var expr = MathExpr.FromCommands(organism.Commands, inputLabels);
            Console.WriteLine("LaTeX:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(expr.AsLatexString());
            Console.ResetColor();
            Console.WriteLine();

            //Console.WriteLine("Traditional string:");
            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine(expr.AsTraditionalString());
            //Console.ResetColor();
            //Console.WriteLine();
        }
    }

    static void ReadInputLabelsFromUser(string[] inputLabels)
    {
        for (var i = 0; i < inputLabels.Length; i++)
        {
            Console.Write($"Input label {i}: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            inputLabels[i] = Console.ReadLine()!.Trim();
            Console.ResetColor();
        }
    }
}
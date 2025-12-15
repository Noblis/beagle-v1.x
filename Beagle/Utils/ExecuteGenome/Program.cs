using BeagleLib.Agent;
using BeagleLib.MathStackLib;
using BeagleLib.VM;
using Newtonsoft.Json;

namespace ExecuteGenome
{
    public class Program
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

            Console.WriteLine("Welcome to Execute Genome!");
            Console.WriteLine("Please enter a genome:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var json = Console.ReadLine()!;
            Console.ResetColor();

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

            Console.WriteLine("Traditional string:");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(expr.AsTraditionalString());
            Console.ResetColor();
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Please enter inputs:");
                var inputs = ReadInputsFromUser(inputLabels);
                var output = new CodeMachine().RunCommands(inputs, organism.Commands);
                Console.WriteLine($"Output: {output}");
                Console.WriteLine();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        static float ReadFloat()
        {
            while(true)
            {
                try
                {
                    var inputStr = Console.ReadLine();
                    var input = float.Parse(inputStr!);
                    return input;
                }
                catch(Exception)
                {
                    Console.Write("Invalid input. Try again: ");
                }
            }
        }

        static float[] ReadInputsFromUser(string[] inputLabels)
        {
            var inputs = new float[inputLabels.Length];
            for (var i = 0; i < inputLabels.Length; i++)
            {
                Console.Write($"Enter {inputLabels[i]}: ");
                inputs[i] = ReadFloat();
            }
            return inputs;
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
}


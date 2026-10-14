using System.Text;
using BeagleLib.Engine;

namespace MLSetup2File;

public static class SyntheticDataHelper
{
    public static void Create<TMLSetup>(int numOfRecords, FileOutputType fileOutputType = FileOutputType.Csv)
        where TMLSetup : MLSetup, new()
    {
        MLSetup mlSetup = new TMLSetup();
        var output = "";
        var extension = "";

        switch (fileOutputType)
        {
            case FileOutputType.Csv:
            {
                output = GenerateCsv(mlSetup, numOfRecords);
                extension = ".csv";
                break;
            }
            case FileOutputType.Json:
            {
                output = GenerateJson(mlSetup, numOfRecords);
                extension = ".json";
                break;
            }
        }

        const string dirName = "SyntheticData";
        var fileName = Path.Combine(dirName, mlSetup.GetType().Name + extension);

        if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
        if (File.Exists(fileName)) File.Delete(fileName);
        var file = File.Open(fileName, FileMode.Create);

        using StreamWriter writer = new StreamWriter(file);
        writer.Write(output);
    }

    static string GenerateCsv(MLSetup mlSetup, int numOfInputSets)
    {
        var buffer = new StringBuilder();

        for (int i = 0; i < mlSetup.GetInputLabels().Length; i++)
        {
            buffer.Append($"{mlSetup.GetInputLabels()[i]},");
        }

        buffer.Append("output\n");

        float[,] allInputsToFill = new float[numOfInputSets, mlSetup.GetInputLabels().Length];
        for (ushort inputSetIdx = 0; inputSetIdx < numOfInputSets; inputSetIdx++)
        {
            var (_, output) = mlSetup.GetNextInputsAndCorrectOutput(allInputsToFill, inputSetIdx);
            for(var i = 0; i < allInputsToFill.GetLength(1); i++)
            {
                buffer.Append($"{allInputsToFill[inputSetIdx, i]},");
            }
            buffer.Append($"{output}\n");
        }

        return buffer.ToString();
    }
    static string GenerateJson(MLSetup mlSetup, int numOfInputSets)
    {
        var buffer = new StringBuilder();

        float[,] allInputsToFill = new float[1, mlSetup.GetInputLabels().Length];
        buffer.Append("[\n");
        for (ushort inputSetIdx = 0; inputSetIdx < numOfInputSets; inputSetIdx++)
        {
            buffer.Append("{");
            var (_, output) = mlSetup.GetNextInputsAndCorrectOutput(allInputsToFill, inputSetIdx);
            for (int i = 0; i < mlSetup.GetInputLabels().Length; i++)
            {
                buffer.Append($"\"{mlSetup.GetInputLabels()[i]}\": {allInputsToFill[inputSetIdx, i]},");
            }
            buffer.Append($"\"output\": {output}");
            buffer.Append("}\n");

            if (inputSetIdx + 1 < numOfInputSets)
            {
                buffer.Append(",");
            }
        }

        buffer.Append("]\n");

        return buffer.ToString();
    }
}
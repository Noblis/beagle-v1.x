using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
using MathNet.Numerics;
using Newtonsoft.Json;

namespace BeagleLib.Agent;

public class Organism
{
    #region Constructors
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Organism CreateByRandomLoadOrConstCommandThenMutate(byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        Span<Command> mutationCommands = stackalloc Command[BConfig.MaxScriptLength];
        var mutationCommandsLength = 0;

        //mutationCommands.Add(ref mutationCommandsLength, Command.CreateRandomLoad(inputsCount));
        mutationCommands.Add(ref mutationCommandsLength, Command.CreateRandomLoadOrConst(inputsCount));
        mutationCommands.Mutate(ref mutationCommandsLength, inputsCount, allowedOperations, allowedAdjunctOperationsCount);
        var result = CreateByCopyingCommandsFromPartOfSpan(mutationCommands, mutationCommandsLength);

        return result;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Organism CreateByCopyingCommandsFromPartOfSpan(Span<Command> other, int otherLength)
    {
        var organism = LoadOrganismFromDeadPoolOrCreate(otherLength);
        for (var i = 0; i < otherLength; i++)
        {
            organism.Commands[i] = other[i];
        }
        return organism;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Organism CreateFromCommands(params Command[] commands)
    {
        var organism = new Organism(commands);
        return organism;
    }

    //This method circumvents the dead organism pool for both creating and destroying an organism.
    //It is meant to only be used for thread-safe data exchange between MLEngine and another thread.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Organism CloneForExport()
    {
        //allocate array of the same size
        var commandsDeepCopy = new Command[Commands.Length];
        
        //Deep copy of commands 
        for (var i = 0; i < Commands.Length; i++)
        {
            commandsDeepCopy[i] = Commands[i];
        }

        //Copy fields and properties
        var organism = new Organism(commandsDeepCopy)
        {
            Score = Score,
            TaxedScore = TaxedScore,
            _asr = _asr
        };

        return organism;
    }

    public static Organism CreateFromAnyString(string str)
    {
        if (str.StartsWith("[")) return CreateFromJson(str);
        else return CreateFromGCLAssembly(str);
    }
    public static Organism CreateFromJson(string json)
    {
        var commands = JsonConvert.DeserializeObject<Command[]>(json);
        var commandsSpan = new Span<Command>(commands);
        commandsSpan.VerifyScriptValid(commandsSpan.Length, false);
        return CreateFromCommands(commands!);
    }
    public static Organism CreateFromGCLAssembly(string stdFormat)
    {
        var isInlineFormat = stdFormat.First() != '1';

        var parts =
            // On else we are removing the leading numbers and then colon, which indicates the line number
            isInlineFormat ? 
            stdFormat.Split(';').Select(part => part.Trim()).ToArray() : 
            stdFormat.Split('\n').Select(part => part.Trim()).Select(part => part.Substring(part.IndexOf(':') + 1).Trim()).ToArray();
        
        var commands = new List<Command>();
        foreach (string part in parts)
        {
            // If it's an argument holding command we can get the second part
            var parts2 = part.Split(' ');
            var idx = float.Pi;
            if (parts2.Length > 1)
            {
                var argument = parts2[1];
                if (argument.Contains(":"))
                {
                    argument = argument.Split(":")[1];
                }
                else if (argument.Contains("@"))
                {
                    argument = argument.Split("@")[1];
                }
                idx = float.Parse(argument);
            }
            
            switch (parts2.First())
            {
                case "ADD":
                {
                    commands.Add(new Command(OpEnum.Add));
                    break;
                }
                case "CONST":
                {
                    commands.Add(new Command(OpEnum.Const, idx));
                    break;
                }
                case "DIV":
                {
                    commands.Add(new Command(OpEnum.Div));
                    break;
                }
                case "DUP":
                {
                    commands.Add(new Command(OpEnum.Dup));
                    break;
                }
                case "DEL":
                {
                    commands.Add(new Command(OpEnum.Del));
                    break;
                }
                case "LOAD":
                {
                    commands.Add(new Command(OpEnum.Load, (int)idx));
                    break;
                }
                case "MUL":
                {
                    commands.Add(new Command(OpEnum.Mul));
                    break;
                }
                case "SIGN":
                {
                    commands.Add(new Command(OpEnum.Sign));
                    break;
                }
                case "SQRT":
                {
                    commands.Add(new Command(OpEnum.Sqrt));
                    break;
                }
                case "CBRT":
                {
                    commands.Add(new Command(OpEnum.Cbrt));
                    break;
                }
                case "SUB":
                {
                    commands.Add(new Command(OpEnum.Sub));
                    break;
                }
                case "SWAP":
                {
                    commands.Add(new Command(OpEnum.Swap));
                    break;
                }
                case "PASTE":
                {
                    commands.Add(new Command(OpEnum.Paste, (int)idx));
                    break;
                }
                case "SQUARE":
                {
                    commands.Add(new Command(OpEnum.Square));
                    break;
                }
                case "CUBE":
                {
                    commands.Add(new Command(OpEnum.Cube));
                    break;
                }
                case "LN":
                {
                    commands.Add(new Command(OpEnum.Ln));
                    break;
                }
                case "SIN":
                {
                    commands.Add(new Command(OpEnum.Sin));
                    break;
                }
                case "POW":
                {
                    commands.Add(new Command(OpEnum.Pow));
                    break;
                }
                case "COPY":
                {
                    commands.Add(new Command(OpEnum.Copy, (int)idx));
                    break;
                }
            }
        }
        var commandsSpan = new Span<Command>(commands.ToArray());
        commandsSpan.VerifyScriptValid(commandsSpan.Length, false);
        return CreateFromCommands(commands.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    protected Organism(int commandsLength) :this(new Command[commandsLength]) { }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Organism(Command[] commands)
    {
        ResetPropertiesForNewOrganism();
        Commands = commands;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ResetPropertiesForNewOrganism()
    {
        LinearRegressionDone = false;
        _scale = 1;
        _offset = 0;

        Score = TaxedScore = 0;
        _asr = null;
    }

    static Organism()
    {
        _organismDeadPools = new ConcurrentStack<Organism>[BConfig.MaxScriptLength];
        for(var i = 0; i < _organismDeadPools.Length; i++) _organismDeadPools[i] = new ConcurrentStack<Organism>();
    }
    #endregion

    #region Overrides
    public string ToString(string[] inputLabels)
    {
        _sb.Clear();
        _sb.Append($"Length: {Commands.Length} TaxedScore: {TaxedScore}: Score: {Score} | ");
        for (var addr = 0; addr < Commands.Length; addr++)
        {
            _sb = Commands[addr].AppendToStringBuilder(inputLabels, _sb);
            _sb.Append("; ");
        }
        return _sb.ToString();
    }
    public override string ToString()
    {
        _sb.Clear();
        _sb.Append($"Length: {Commands.Length} TaxedScore: {TaxedScore}: Score: {Score} | ");
        for (var addr = 0; addr < Commands.Length; addr++)
        {
            _sb.Append($"{Commands[addr].ToString()}; ");
        }
        return _sb.ToString();
    }
    #endregion

    #region Methods
    public Organism ProduceMutatedChild(byte inputsCount, OpEnum[] allowedOperations, int allowedAdjunctOperationsCount)
    {
        //Copy Organisms Commands into mutationCommands Span
        Span<Command> mutationCommands = stackalloc Command[BConfig.MaxScriptLength];
        var mutationCommandsLength = Commands.Length;

        Commands.CopyTo(mutationCommands);

        #if DEBUG
        //Verify that it copied correctly
        if (Commands.Length != mutationCommandsLength) ReportInvalidScriptAndBreak();

        for (var i = 0; i < Commands.Length; i++)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Commands[i].Operation != mutationCommands[i].Operation ||
                Commands[i].CommandType == CommandTypeEnum.CommandPlusFloat && Commands[i].ConstValue != mutationCommands[i].ConstValue ||
                Commands[i].CommandType == CommandTypeEnum.CommandPlusIndex && Commands[i].Idx != mutationCommands[i].Idx)
            {
                ReportInvalidScriptAndBreak();
            }
        }
        #endif

        mutationCommands.Mutate(ref mutationCommandsLength, inputsCount, allowedOperations, allowedAdjunctOperationsCount);
        return CreateByCopyingCommandsFromPartOfSpan(mutationCommands, mutationCommandsLength);
    }
    public List<Command> GetTrueCommandList
    #endregion

    #region Print and ToJson Commands
    public void PrintCommands(string[] inputLabels, float[][] inputsArray, float[] correctOutputs)
    {
        CalcScaleAndOffsetIfNeeded(inputsArray, correctOutputs);
        
        int addr;
        for (addr = 0; addr < Commands.Length; addr++)
        {
            _sb.Clear();
            Output.WriteLine($"{addr + 1}: {Commands[addr].AppendToStringBuilder(inputLabels, _sb)}");
        }

        if (MLSetup.IsCorrelationFunctionRun)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Scale != 1)
            {
                _sb.Clear();
                Output.WriteLine($"{addr + 1}: {new Command(OpEnum.Const, Scale).AppendToStringBuilder(inputLabels, _sb)}");

                _sb.Clear();
                Output.WriteLine($"{addr + 1}: {new Command(OpEnum.Mul).AppendToStringBuilder(inputLabels, _sb)}");
            }

            if (Offset != 0)
            {
                _sb.Clear();
                Output.WriteLine($"{addr + 1}: {new Command(OpEnum.Const, Offset).AppendToStringBuilder(inputLabels, _sb)}");

                _sb.Clear();
                Output.WriteLine($"{addr + 1}: {new Command(OpEnum.Add).AppendToStringBuilder(inputLabels, _sb)}");
            }
        }
    }
    public void PrintCommandsInLine(string[] inputLabels, float[][] inputsArray, float[] correctOutputs)
    {
        CalcScaleAndOffsetIfNeeded(inputsArray, correctOutputs);

        for (var addr = 0; addr < Commands.Length; addr++)
        {
            _sb.Clear();
            Output.Write($"{Commands[addr].AppendToStringBuilder(inputLabels, _sb)}; ");
        }

        if (MLSetup.IsCorrelationFunctionRun)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Scale != 1)
            {
                _sb.Clear();
                Output.Write($"{new Command(OpEnum.Const, Scale).AppendToStringBuilder(inputLabels, _sb)}; ");

                _sb.Clear();
                Output.Write($"{new Command(OpEnum.Mul).AppendToStringBuilder(inputLabels, _sb)}; ");
            }

            if (Offset != 0)
            {
                _sb.Clear();
                Output.Write($"{new Command(OpEnum.Const, Offset).AppendToStringBuilder(inputLabels, _sb)}; ");

                _sb.Clear();
                Output.Write($"{new Command(OpEnum.Add).AppendToStringBuilder(inputLabels, _sb)}; ");
            }
        }

        Output.WriteLine("");
    }
    
    public string CommandsToJson(float[][] inputsArray, float[] correctOutputs)
    {
        CalcScaleAndOffsetIfNeeded(inputsArray, correctOutputs);
        var commandsList = Commands.ToList();

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Scale != 1)
        {
            commandsList.Add(new Command(OpEnum.Const, _mostAccurateEverOrganism.Scale));
            commandsList.Add(new Command(OpEnum.Mul));
        }

        if (Offset != 0)
        {
            commandsList.Add(new Command(OpEnum.Const, _mostAccurateEverOrganism.Offset));
            commandsList.Add(new Command(OpEnum.Add));
        }

        return JsonConvert.SerializeObject(Commands);
    }
    #endregion

    #region Diagnistics Helpers
    private void ReportInvalidScriptAndBreak()
    {
        Notifications.SendSystemMessageSMTP(BConfig.ToEmail, $"Beagle 1.7: Invalid script copy detected on {Environment.MachineName}!", "", MailPriority.High);
        Debugger.Break();
    }
    #endregion

    #region Methods and Properties Related to Dead Pool, pool of Organism that can be reused (to reduce GC usage)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Organism LoadOrganismFromDeadPoolOrCreate(int commandsLength)
    {
        Debug.Assert(commandsLength >= 1);

        if (_organismDeadPools[commandsLength - 1].TryPop(out var organism))
        {
            Debug.Assert(organism.Commands.Length == commandsLength);
            organism.ResetPropertiesForNewOrganism();
            
            return organism;
        }
        
        return new Organism(commandsLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SaveOrganismToDeadPool(Organism organism)
    {
        _organismDeadPools[organism.Commands.Length - 1].Push(organism);
    }

    private static readonly ConcurrentStack<Organism>[] _organismDeadPools;
    #endregion

    #region Lieaner Regression Methods
    public void CalcScaleAndOffsetIfNeeded(float[][] inputsArray, float[] correctOutputs)
    {
        if (!MLSetup.IsCorrelationFunctionRun) throw new InvalidOperationException("Cannot call SetScaleAndOffset when IsCorrelationFunctionRun is false");

        if (!LinearRegressionDone)
        {
            _dblCorrectOutputs ??= new double[correctOutputs.Length];
            _dblOutputs ??= new double[correctOutputs.Length];

            Parallel.For(0, correctOutputs.Length, i =>
            {
                _dblCorrectOutputs[i] = correctOutputs[i];
                _dblOutputs[i] = new CodeMachine().RunCommands(inputsArray[i], Commands);
            });

            (double, double) lineRegression = Fit.Line(_dblOutputs, _dblCorrectOutputs);
            var offset = (float)lineRegression.Item1;
            var scale = (float)lineRegression.Item2;

            SetScaleAndOffset(scale, offset);
        }
    }
    public void SetScaleAndOffset(float scale, float offset)
    {
        if (!MLSetup.IsCorrelationFunctionRun) throw new InvalidOperationException("Cannot call SetScaleAndOffset when IsCorrelationFunctionRun is false");

        LinearRegressionDone = true;
        _scale = scale;
        _offset = offset;
    }
    #endregion

    #region Properties
    public bool LinearRegressionDone { get; protected set; }

    public float Scale
    {
        get
        {
            if (!MLSetup.IsCorrelationFunctionRun) throw new InvalidOperationException("Cannot read Scale when IsCorrelationFunctionRun is false");
            if (!LinearRegressionDone) throw new InvalidOperationException("Cannot read Scale when LinearRegressionDone is false");
            return _scale;
        }
    }
    protected float _scale;

    public float Offset
    {
        get
        {
            if (!MLSetup.IsCorrelationFunctionRun) throw new InvalidOperationException("Cannot read Offset when IsCorrelationFunctionRun is false");
            if (!LinearRegressionDone) throw new InvalidOperationException("Cannot read Offset when LinearRegressionDone is false");
            return _offset;
        }
    }
    protected float _offset;

    public Command[] Commands { get; protected set; }
    public int Score { get; set; }
    public int TaxedScore { get; set; }

    public double ASR
    {
        get
        {
            _asr ??= Math.Round((double)Score / MLSetup.MaxGenerationScore, 4);
            return _asr.Value;
        }
    }
    private double? _asr;

    private static StringBuilder _sb = new(8192); //buffer to build strings

    //buffers for linear regression 
    private static double[]? _dblCorrectOutputs;
    private static double[]? _dblOutputs;
    #endregion
}
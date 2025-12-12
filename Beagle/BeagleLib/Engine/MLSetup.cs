using BeagleLib.Util;
using BeagleLib.VM;

namespace BeagleLib.Engine;

public abstract class MLSetup
{
    #region Constructors
    protected MLSetup()
    {
        Current = this;

        // ReSharper disable VirtualMemberCallInConstructor
        MaxGenerationScore = (int)(BConfig.MaxScore * ExperimentsPerGeneration);
        SolutionFoundGenerationScore = (int)(MaxGenerationScore * SolutionFoundASRThreshold);
        // ReSharper restore VirtualMemberCallInConstructor
    }
    #endregion

    #region Abstract & Virtual Methods and Properties
    public abstract string[] GetInputLabels();
    public abstract (float[], float) GetNextInputsAndCorrectOutput(float[] inputsToFill);

    public virtual int CalcScriptLengthTax(int scriptLength) => scriptLength * ScriptLengthTaxRate;
    public virtual double SolutionFoundASRThreshold => 1.0;
    #endregion

    #region Virtual Methods and Properties
    public virtual string Name => GetType().Name;

    public virtual OpEnum[] GetAllowedOperations()
    {
        var operationEnumValues = Enum.GetValues(typeof(OpEnum));
        var allowedOperations = new OpEnum[operationEnumValues.Length - 1]; //we do -1 because the first command is EndOfScript 

        for (var i = 0; i < operationEnumValues.Length - 1; i++)
        {
            allowedOperations[i] = (OpEnum)operationEnumValues.GetValue(i + 1)!;
        }

        return allowedOperations;
    }
    public virtual int GetAllowedAdjunctOperationsCount() => 1; //1 for Copy command

    public virtual int TargetColonySize(int generation)
    {
        if (generation % 1500 < 20) return 3_000_000;
        return 250_000;
    }
    public int OrganismsArraySize
    {
        get
        {
            if (!_organismArraySize.HasValue)
            {
                int max = 0;
                for (var i = 0; i < 10_000_000; i++)
                {
                    if (TargetColonySize(i) > max) max = TargetColonySize(i);
                }
                _organismArraySize = checked(max * 4);
            }
            return _organismArraySize.Value;
        }
    }
    protected int? _organismArraySize;

    public virtual uint ExperimentsPerGeneration => 512;
    public virtual long TotalBirthsToResetColonyIfNoProgress => 25_000_000_000;

    protected int ScriptLengthTaxRate
    {
        get
        {
            _scriptLengthTaxRate ??= ScriptLengthTaxRateInternal;
            return _scriptLengthTaxRate.Value;
        }
    }
    private int? _scriptLengthTaxRate;
    protected virtual int ScriptLengthTaxRateInternal => BConfig.MaxScore * (int)ExperimentsPerGeneration / 200;

    public virtual bool KeepOptimizingAfterSolutionFound => false;
    #endregion

    #region Settings Proprties
    public static MLSetup Current { get; private set; } = null!;
    public static int SolutionFoundGenerationScore { get; private set; }
    public static int MaxGenerationScore { get; private set; }
    public static bool IsCorrelationFunctionRun { get; set; }
    #endregion
}
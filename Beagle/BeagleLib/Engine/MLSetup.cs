using BeagleLib.Util;
using BeagleLib.VM;

namespace BeagleLib.Engine;

public abstract class MLSetup
{
    #region Constructors
    protected MLSetup()
    {
        Current = this;

        // ReSharper disable once VirtualMemberCallInConstructor
        MaxGenerationScore = (int)(BConfig.MaxScore * ExperimentsPerGeneration);
    }
    #endregion

    #region Abstract & Virtual Methods and Properties
    public abstract string[] GetInputLabels();
    public abstract (float[], float) GetNextInputsAndCorrectOutput(float[] inputsToFill);
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
        if (generation < 20) return 5_000_000;
        return 300_000; //250_000;
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
    public virtual long TotalBirthsToResetColonyIfNoProgress => 10_000_000_000;

    public virtual bool RemoveRedundantCommandsAfterMutation => true;
    public virtual double CrossoverRate => 0.3;    
    #endregion

    #region Settings Proprties
    public static MLSetup Current { get; private set; } = null!;
    public static int MaxGenerationScore { get; private set; }
    public static bool IsCorrelationFunctionRun { get; set; }
    #endregion
}
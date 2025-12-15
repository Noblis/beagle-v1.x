using BeagleLib.Util;
using ILGPU;

namespace BeagleLib.Engine.FitFunc;

public struct StdFitFunc : IFitFunc
{
    public bool UseCorrelationFit => false;

    public int FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput)
    {
        return FitFunction(output, correctOutput);
    }

    public int FitFunction(float output, float correctOutput)
    {
        var absOutput = Math.Abs(output);
        var absCorrectOutput = Math.Abs(correctOutput);

        var smallerAbs = Math.Min(absOutput, absCorrectOutput);
        var biggerAbs = Math.Max(absOutput, absCorrectOutput);

        float ratio;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (biggerAbs == 0) ratio = 1; //this means smallerAbs is zero too
        else ratio = smallerAbs / biggerAbs;

        var signPenaltyValue = BConfig.MaxScore / 11;
        var signPenalty = output * correctOutput < 0 ? signPenaltyValue : 0;
        var reward = (int)(BConfig.MaxScore * ratio - signPenalty + 0.5f);
        if (reward > BConfig.MaxScore) reward = BConfig.MaxScore;

        return reward;
    }

    public int FitFunctionIfInvalid(bool isOutputValid, bool isCorrectOutputValid)
    {
        //XOR return true if different, false if the same
        if (isOutputValid ^ isCorrectOutputValid) return -BConfig.MaxScore;
        else return BConfig.MaxScore;
    }
}
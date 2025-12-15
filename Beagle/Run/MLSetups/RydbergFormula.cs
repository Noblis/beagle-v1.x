using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
using NumSharp;

namespace Run.MLSetups;

public class RydbergFormula : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {

        int n1 = Rnd.Random.Next(1, 7);
        int n2 = Rnd.Random.Next(n1 + 1, 8);

        double n1F = inputs[0] = n1;
        double n2F = inputs[1] = n2;

        const double rh = 1.097e7; //109677.57f;
        double v = rh*(1/(n1F*n1F) - 1/(n2F*n2F));

        //add noise the way Miles Cranmer does it
        //const double scale = 0.01f;
        //double randn = _rs.randn();
        //double delta = randn * v * scale;
        //v += delta;

        return (inputs, (float)v);
    }
    public override string[] GetInputLabels()
    {
        return ["n1", "n2"];
    }
    //public override OpEnum[] GetAllowedOperations()
    //{
    //    return base.GetAllowedOperations().Where(x => x != OpEnum.Sin && x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Ln).ToArray();
    //}
    public override uint ExperimentsPerGeneration => 512;
    public override long TotalBirthsToResetColonyIfNoProgress => 750_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;

    //private readonly NumPyRandom _rs = np.random.RandomState();
    #endregion
}
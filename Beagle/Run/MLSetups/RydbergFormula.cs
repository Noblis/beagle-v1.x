using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class RydbergFormula : MLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {

        int n1 = Rnd.Random.Next(1, 7);
        int n2 = Rnd.Random.Next(n1 + 1, 8);

        float n1F = allInputs[inputSetIdx, 0] = n1;
        float n2F = allInputs[inputSetIdx, 1] = n2;

        const float rh = 1.097e7f; //109677.57f;
        float v = rh*(1/(n1F*n1F) - 1/(n2F*n2F));

        //add noise the way Miles Cranmer does it
        //const double scale = 0.01f;
        //double randn = _rs.randn();
        //double delta = randn * v * scale;
        //v += delta;

        return (allInputs, v);
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
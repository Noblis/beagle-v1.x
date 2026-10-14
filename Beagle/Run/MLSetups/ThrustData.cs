using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class ThrustData : MLSetup
{
    #region Overrides
    public override string[] GetInputLabels()
    {
        return ["h", "M"];
    }
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        while (true)
        {
            var hIdx = Rnd.Random.Next(_hs.Length);
            var mIdx = Rnd.Random.Next(_ms.Length);

            var output = _data[hIdx, mIdx];
            if (output.HasValue)
            {
                allInputs[inputSetIdx, 0] = _hs[hIdx]; 
                allInputs[inputSetIdx, 1] = _ms[mIdx];

                return (allInputs, output.Value);
            }
        }
    }
    public override uint ExperimentsPerGeneration => 1024;
    public override long TotalBirthsToResetColonyIfNoProgress => 6_000_000_000;
    #endregion

    #region Data
    private static readonly float[] _hs = [0, 5, 10, 15, 20, 25, 30, 40, 50, 70];
    private static readonly float[] _ms = [0, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f, 1.2f, 1.4f, 1.6f, 1.8f];
    private static readonly float?[,] _data =
    {
        { 24.2f, null,  null,  null,  null,  null,  null,  null,  null,  null },
        { 28f,   24.6f, 21.1f, 18.1f, 15.2f, 12.8f, 10.7f, null,  null,  null },
        { 28.3f, 25.2f, 21.9f, 18.7f, 15.9f, 13.4f, 11.2f, null,  null,  null },
        { 30.8f, 27.2f, 23.8f, 20.5f, 17.3f, 14.7f, 12.3f, 8.1f,  4.9f,  null },
        { 34.5f, 30.3f, 26.6f, 23.2f, 19.8f, 16.8f, 14.1f, 9.4f,  5.6f,  1.1f },
        { 37.9f, 34.3f, 30.4f, 26.8f, 23.3f, 19.8f, 16.8f, 11.2f, 6.8f,  1.4f },
        { 36.1f, 38f,   34.9f, 31.3f, 27.3f, 23.6f, 20.1f, 13.4f, 8.3f,  1.7f },
        { null,  36.6f, 38.5f, 36.1f, 31.6f, 28.1f, 24.2f, 16.2f, 10f,   2.2f },
        { null,  null,  null,  38.7f, 35.7f, 32f,   28.1f, 19.3f, 11.9f, 2.9f },
        { null,  null,  null,  null,  null,  34.6f, 31.1f, 21.7f, 13.3f, 3.1f }
    };
    #endregion
}
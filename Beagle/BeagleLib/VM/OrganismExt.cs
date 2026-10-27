using BeagleLib.Agent;
using System.Runtime.CompilerServices;

namespace BeagleLib.VM;

public static class OrganismExt
{
    #region Comparision Methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMoreAccurateOrSameAccuracyButShorterThanMe(this Organism? me, Organism other)
    {
        return me == null ||
               me.Score < other.Score || me.Score <= other.Score && me.Commands.Length > other.Commands.Length;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static bool IsSatisfactoryAccuracyAndShorterOrSameLengthButMoreAccurateThanMe(this Organism? me, Organism other, double solutionFoundASRThreshold)
    //{
    //    return other.ASR >= solutionFoundASRThreshold && 
    //           (me == null ||
    //            me.ASR < other.ASR && me.Commands.Length >= other.Commands.Length || //ASR is rounded and it should also be better
    //            me.ASR <= other.ASR && me.Commands.Length > other.Commands.Length);
    //}
    #endregion
}
using System.Runtime.CompilerServices;

namespace BeagleLib.Util;

public static class NaNHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidNumber(this float me)
    {
        return !float.IsNaN(me) && !float.IsInfinity(me) && !float.IsNegativeInfinity(me);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidNumber(this double me)
    {
        return !double.IsNaN(me) && !double.IsInfinity(me) && !double.IsNegativeInfinity(me);
    }
}
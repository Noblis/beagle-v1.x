
namespace BeagleLib.Util;

public static class SigFigHelper
{
    public static double RoundToSignificantDigits(this double d, int digits)
    {
        if (d == 0) return 0;

        double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
        return scale * Math.Round(d / scale, digits);
    }

    public static double TruncateToSignificantDigits(this double d, int digits)
    {
        if (d == 0) return 0;

        double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1 - digits);
        return scale * Math.Truncate(d / scale);
    }
}

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

    public static float RoundToSignificantDigits(this float d, int digits)
    {
        if (d == 0) return 0;

        float scale = MathF.Pow(10, MathF.Floor(MathF.Log10(MathF.Abs(d))) + 1);
        return scale * MathF.Round(d / scale, digits);
    }

    public static float TruncateToSignificantDigits(this float d, int digits)
    {
        if (d == 0) return 0;

        float scale = MathF.Pow(10, MathF.Floor(MathF.Log10(MathF.Abs(d))) + 1 - digits);
        return scale * MathF.Truncate(d / scale);
    }

}
namespace BeagleLib.Util;

public static class MyInterlocked
{
    #region Methods
    public static double Add(ref double location1, double value)
    {
        double newCurrentValue = location1; // non-volatile read, so may be stale
        while (true)
        {
            double currentValue = newCurrentValue;
            double newValue = currentValue + value;
            newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
            if (newCurrentValue.Equals(currentValue)) return newValue;
        }
    }
    public static float Add(ref float location1, float value)
    {
        float newCurrentValue = location1; // non-volatile read, so may be stale
        while (true)
        {
            float currentValue = newCurrentValue;
            float newValue = currentValue + value;
            newCurrentValue = Interlocked.CompareExchange(ref location1, newValue, currentValue);
            if (newCurrentValue.Equals(currentValue)) return newValue;
        }
    }
    #endregion
}
using System;

namespace MercenaryManagement.Services;

internal static class MercenaryValuePolicy
{
    public static float NormalizePercentage(float value, float fallback = 50f)
    {
        float finiteFallback = float.IsNaN(fallback) || float.IsInfinity(fallback)
            ? 50f
            : fallback;
        float finiteValue = float.IsNaN(value) || float.IsInfinity(value)
            ? finiteFallback
            : value;
        return Math.Max(0f, Math.Min(100f, finiteValue));
    }

    public static float AdjustPercentage(float current, float delta)
    {
        float safeCurrent = NormalizePercentage(current);
        if (float.IsNaN(delta) || float.IsInfinity(delta))
            return safeCurrent;
        return NormalizePercentage(safeCurrent + delta, safeCurrent);
    }

    public static int AccumulateNonNegative(int current, int delta)
    {
        long total = (long)Math.Max(0, current) + delta;
        if (total <= 0L) return 0;
        return total >= int.MaxValue ? int.MaxValue : (int)total;
    }
}

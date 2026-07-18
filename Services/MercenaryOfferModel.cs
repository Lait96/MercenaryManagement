using System;

namespace MercenaryManagement.Services;

internal readonly struct OfferTermsResult
{
    public OfferTermsResult(int influencePrice, int durationDays)
    {
        InfluencePrice = influencePrice;
        DurationDays = durationDays;
    }

    public int InfluencePrice { get; }
    public int DurationDays { get; }
}

internal static class MercenaryOfferModel
{
    public static OfferTermsResult CalculateTerms(int basePrice, int minimumPrice,
        int maximumPrice, float militaryNeed, float reliability, bool requested)
    {
        int lowerBound = Math.Max(0, Math.Min(minimumPrice, maximumPrice));
        int upperBound = Math.Max(lowerBound, Math.Max(minimumPrice, maximumPrice));
        int safeBasePrice = Math.Max(0, basePrice);
        float safeMilitaryNeed = Bounded(militaryNeed, 0f, 1f, 0f);
        float safeReliability = Bounded(reliability, 0f, 100f, 50f);
        float multiplier = 0.9f + safeMilitaryNeed * 0.25f
                           + (safeReliability - 50f) / 500f;
        if (requested) multiplier -= 0.05f;
        double calculatedPrice = Math.Round(safeBasePrice * (double)multiplier);
        int price = calculatedPrice <= lowerBound
            ? lowerBound
            : calculatedPrice >= upperBound
                ? upperBound
                : (int)calculatedPrice;
        int duration = safeMilitaryNeed >= 0.75f ? 120
            : safeMilitaryNeed >= 0.4f ? 60 : 30;
        return new OfferTermsResult(price, duration);
    }

    private static float Bounded(float value, float minimum, float maximum, float fallback)
    {
        float finite = float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
        return Math.Max(minimum, Math.Min(maximum, finite));
    }
}

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
        float multiplier = 0.9f + militaryNeed * 0.25f
                           + (reliability - 50f) / 500f;
        if (requested) multiplier -= 0.05f;
        int price = Math.Max(minimumPrice,
            Math.Min(maximumPrice, (int)Math.Round(basePrice * multiplier)));
        int duration = militaryNeed >= 0.75f ? 120 : militaryNeed >= 0.4f ? 60 : 30;
        return new OfferTermsResult(price, duration);
    }
}

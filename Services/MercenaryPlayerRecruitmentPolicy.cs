using System;

namespace MercenaryManagement.Services;

internal static class MercenaryPlayerRecruitmentPolicy
{
    public const int AcceptanceBonus = 20;
    public const int MinimumAcceptanceChance = 75;
    public const int MaximumAcceptanceChance = 95;
    public const float ProtectedServiceDays = 7f;

    public static int ApplyPlayerRulerAcceptanceAdvantage(int baseChance)
    {
        int normalized = Math.Max(0, Math.Min(100, baseChance));
        return Math.Min(MaximumAcceptanceChance,
            Math.Max(MinimumAcceptanceChance, normalized + AcceptanceBonus));
    }

    public static bool HasCompletedProtectedService(float elapsedServiceDays)
    {
        return !float.IsNaN(elapsedServiceDays)
               && !float.IsInfinity(elapsedServiceDays)
               && elapsedServiceDays >= ProtectedServiceDays;
    }
}

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryPartyItemVM : ViewModel
{
    private readonly MobileParty _party;

    public MercenaryPartyItemVM(MobileParty party)
    {
        _party = party;
    }

    [DataSourceProperty] public string PartyName => _party.Name?.ToString() ?? string.Empty;
    [DataSourceProperty] public string LeaderName => _party.LeaderHero?.Name.ToString() ?? string.Empty;
    [DataSourceProperty] public int TroopCount => _party.MemberRoster.TotalManCount;
    [DataSourceProperty] public string Activity => GetActivity(_party);

    public override void RefreshValues()
    {
        base.RefreshValues();
        OnPropertyChangedWithValue(PartyName, nameof(PartyName));
        OnPropertyChangedWithValue(LeaderName, nameof(LeaderName));
        OnPropertyChangedWithValue(TroopCount, nameof(TroopCount));
        OnPropertyChangedWithValue(Activity, nameof(Activity));
    }

    private static string GetActivity(MobileParty party)
    {
        if (party.MapEvent is not null)
            return Localize("MercenaryManagement_PartyActivityBattle", "In battle");
        if (party.SiegeEvent is not null)
            return Localize("MercenaryManagement_PartyActivitySiege", "Participating in a siege");
        if (party.Army is not null)
            return Localize("MercenaryManagement_PartyActivityArmy", "Serving in an army");

        switch (party.DefaultBehavior)
        {
            case AiBehavior.RaidSettlement:
                return Localize("MercenaryManagement_PartyActivityRaid", "Raiding enemy lands");
            case AiBehavior.BesiegeSettlement:
                return Localize("MercenaryManagement_PartyActivitySiege", "Participating in a siege");
            case AiBehavior.EngageParty:
                return Localize("MercenaryManagement_PartyActivityPursuit", "Pursuing another party");
            case AiBehavior.GoToSettlement:
                return Localize("MercenaryManagement_PartyActivityTravel", "Travelling to a settlement");
            case AiBehavior.PatrolAroundPoint:
                return Localize("MercenaryManagement_PartyActivityPatrol", "Patrolling the area");
            case AiBehavior.EscortParty:
                return Localize("MercenaryManagement_PartyActivityEscort", "Escorting another party");
            default:
                return Localize("MercenaryManagement_PartyActivityIndependent", "Operating independently");
        }
    }

    private static string Localize(string id, string fallback) =>
        new TextObject($"{{={id}}}{fallback}").ToString();
}

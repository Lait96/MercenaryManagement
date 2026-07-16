using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.Commands;

public static class MercenaryDebugCommands
{
    private const string PairSeparator = "|";

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_list_kingdoms", "campaign")]
    public static string ListKingdoms(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (arguments.Count != 0)
        {
            return "Usage: campaign.mm_list_kingdoms";
        }

        StringBuilder result = new StringBuilder("Kingdoms (use id as command input):");
        foreach (Kingdom kingdom in Kingdom.All.OrderBy(
                     candidate => candidate.StringId,
                     StringComparer.Ordinal))
        {
            int mercenaryCount = kingdom.Clans.Count(clan => clan.IsUnderMercenaryService);
            result.AppendLine();
            result.Append("id=").Append(kingdom.StringId)
                .Append(" | name=\"").Append(kingdom.Name).Append('"')
                .Append(" | state=").Append(kingdom.IsEliminated ? "eliminated" : "active")
                .Append(" | mercenary_clans=").Append(mercenaryCount);
        }

        return result.ToString();
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_list_mercenary_clans", "campaign")]
    public static string ListMercenaryClans(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (arguments.Count != 0)
        {
            return "Usage: campaign.mm_list_mercenary_clans";
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        StringBuilder result = new StringBuilder(
            "Mercenary clans (use id and kingdom_id as command input):");
        foreach (Clan clan in Clan.All
                     .Where(candidate => candidate.IsMinorFaction && !candidate.IsBanditFaction)
                     .OrderBy(candidate => candidate.StringId, StringComparer.Ordinal))
        {
            result.AppendLine();
            result.Append("id=").Append(clan.StringId)
                .Append(" | name=\"").Append(clan.Name).Append('"')
                .Append(" | state=").Append(GetMercenaryClanState(clan))
                .Append(" | kingdom_id=")
                .Append(clan.Kingdom?.StringId ?? "none")
                .Append(" | base_rate=").Append(clan.MercenaryAwardMultiplier)
                .Append(" | allowed_rate=")
                .Append(MercenaryPaymentService.CalculateMinimumInfluencePrice(clan))
                .Append("..")
                .Append(MercenaryPaymentService.CalculateMaximumInfluencePrice(clan));

            if (clan.IsUnderMercenaryService && clan.Kingdom is not null)
            {
                result.Append(" | current_rate=")
                    .Append(MercenaryPaymentService.GetInfluencePrice(clan));

                if (contractBehavior is not null
                    && contractBehavior.GetOrCreateContract(
                        clan,
                        out MercenaryContract contract))
                {
                    result.Append(" | start_days_ago=")
                        .Append(contract.ServiceStartTime.ElapsedDaysUntilNow.ToString(
                            "0.##",
                            CultureInfo.InvariantCulture))
                        .Append(" | end=")
                        .Append(FormatContractEnd(contract.EndTime))
                        .Append(" | contract_state=")
                        .Append(contract.State.ToString().ToLowerInvariant());
                }
            }
        }

        return result.ToString();
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_hire_any", "campaign")]
    public static string HireAny(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        string kingdomQuery = JoinArguments(arguments);
        if (string.IsNullOrWhiteSpace(kingdomQuery))
        {
            return "Usage: campaign.mm_hire_any <kingdom name or id>";
        }

        if (!TryFindKingdom(kingdomQuery, out Kingdom kingdom, out error))
        {
            return error;
        }

        Clan? clan = Clan.All
            .Where(candidate =>
                !MercenaryHiringService.TryGetInherentFailureReason(
                    candidate,
                    kingdom,
                    out _))
            .OrderBy(candidate => candidate.StringId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (clan is null)
        {
            return $"No available mercenary clan can enter {kingdom.Name}'s service.";
        }

        return Hire(clan, kingdom);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_hire_clan", "campaign")]
    public static string HireClan(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (!TrySplitPair(arguments, out string kingdomQuery, out string clanQuery))
        {
            return "Usage: campaign.mm_hire_clan <kingdom name or id> | <clan name or id>";
        }

        if (!TryFindKingdom(kingdomQuery, out Kingdom kingdom, out error)
            || !TryFindClan(clanQuery, out Clan clan, out error))
        {
            return error;
        }

        if (MercenaryHiringService.TryGetInherentFailureReason(
                clan,
                kingdom,
                out TextObject reason))
        {
            return $"Could not hire the {clan.Name} clan: {reason}";
        }

        return Hire(clan, kingdom);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_end_service", "campaign")]
    public static string EndService(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        string clanQuery = JoinArguments(arguments);
        if (string.IsNullOrWhiteSpace(clanQuery))
        {
            return "Usage: campaign.mm_end_service <clan name or id>";
        }

        if (!TryFindClan(clanQuery, out Clan clan, out error))
        {
            return error;
        }

        if (!clan.IsMinorFaction || clan.IsBanditFaction)
        {
            return $"The {clan.Name} clan is not a mercenary clan.";
        }

        if (!clan.IsUnderMercenaryService || clan.Kingdom is null)
        {
            return $"The {clan.Name} clan is not currently serving any kingdom as mercenaries.";
        }

        Kingdom previousKingdom = clan.Kingdom!;
        MercenaryDismissalService.EndService(clan);
        if (clan.IsUnderMercenaryService || clan.Kingdom is not null)
        {
            return $"Could not end the {clan.Name} clan's service; the native leave action did not complete.";
        }

        return $"The {clan.Name} clan has left {previousKingdom.Name}'s service.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_set_rate", "campaign")]
    public static string SetRate(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (arguments.Count < 2
            || !int.TryParse(
                arguments[0],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int influencePrice))
        {
            return "Usage: campaign.mm_set_rate <integer rate> <clan name or id>";
        }

        string clanQuery = JoinArguments(arguments.Skip(1));
        if (!TryFindClan(clanQuery, out Clan clan, out error))
        {
            return error;
        }

        if (!MercenaryPaymentService.TrySetInfluencePrice(
                clan,
                influencePrice,
                out TextObject reason))
        {
            return $"Could not change the {clan.Name} clan's rate: {reason}";
        }

        return $"The {clan.Name} clan's individual rate is now {influencePrice} denars per influence.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_set_start_days_ago", "campaign")]
    public static string SetStartDaysAgo(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (arguments.Count < 2
            || !float.TryParse(
                arguments[0],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float daysAgo)
            || daysAgo < 0f
            || float.IsNaN(daysAgo)
            || float.IsInfinity(daysAgo))
        {
            return "Usage: campaign.mm_set_start_days_ago <non-negative days> <clan name or id>";
        }

        string clanQuery = JoinArguments(arguments.Skip(1));
        if (!TryFindClan(clanQuery, out Clan clan, out error))
        {
            return error;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is null)
        {
            return "Mercenary contract storage is not available in the current campaign.";
        }

        CampaignTime startTime = CampaignTime.DaysFromNow(-daysAgo);
        if (!contractBehavior.SetServiceStartTime(clan, startTime))
        {
            return $"Could not change the {clan.Name} clan's service start: the clan has no active mercenary contract.";
        }

        return $"The {clan.Name} clan's service start is now {daysAgo.ToString(CultureInfo.InvariantCulture)} days ago.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_set_contract_days", "campaign")]
    public static string SetContractDays(List<string> arguments)
    {
        if (!TryValidateCampaign(out string error))
        {
            return error;
        }

        if (arguments.Count < 2)
        {
            return "Usage: campaign.mm_set_contract_days <non-negative days | indefinite> <clan name or id>";
        }

        bool isIndefinite = string.Equals(
            arguments[0],
            "indefinite",
            StringComparison.OrdinalIgnoreCase);
        float days = 0f;
        if (!isIndefinite
            && (!float.TryParse(
                    arguments[0],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out days)
                || days < 0f
                || float.IsNaN(days)
                || float.IsInfinity(days)))
        {
            return "Usage: campaign.mm_set_contract_days <non-negative days | indefinite> <clan name or id>";
        }

        string clanQuery = JoinArguments(arguments.Skip(1));
        if (!TryFindClan(clanQuery, out Clan clan, out error))
        {
            return error;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is null)
        {
            return "Mercenary contract storage is not available in the current campaign.";
        }

        CampaignTime endTime = isIndefinite
            ? CampaignTime.Zero
            : CampaignTime.DaysFromNow(days);
        if (!contractBehavior.SetContractEndTime(clan, endTime))
        {
            return $"Could not change the {clan.Name} clan's contract term: the clan has no active mercenary contract.";
        }

        return isIndefinite
            ? $"The {clan.Name} clan's contract is now indefinite."
            : $"The {clan.Name} clan's contract now ends in {days.ToString(CultureInfo.InvariantCulture)} days.";
    }

    private static string Hire(Clan clan, Kingdom kingdom)
    {
        MercenaryHiringService.Hire(clan, kingdom);
        if (!clan.IsUnderMercenaryService || clan.Kingdom != kingdom)
        {
            return $"Could not hire the {clan.Name} clan; the native join action did not complete.";
        }

        return $"The {clan.Name} clan has entered {kingdom.Name}'s service.";
    }

    private static string GetMercenaryClanState(Clan clan)
    {
        if (clan.IsEliminated)
        {
            return "eliminated";
        }

        if (clan.IsUnderMercenaryService)
        {
            return clan.Kingdom is null ? "invalid_service" : "serving";
        }

        return clan.Kingdom is null ? "free" : "kingdom_member";
    }

    private static bool TryValidateCampaign(out string error)
    {
        if (Campaign.Current is null)
        {
            error = "This command can only be used in an active campaign.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryFindKingdom(
        string query,
        out Kingdom kingdom,
        out string error)
    {
        List<Kingdom> matches = FindExactMatches(
            Kingdom.All,
            query,
            candidate => candidate.Name.ToString(),
            candidate => candidate.StringId);
        if (matches.Count == 1)
        {
            kingdom = matches[0];
            if (kingdom.IsEliminated)
            {
                error = $"The {kingdom.Name} kingdom is eliminated.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        kingdom = null!;
        error = matches.Count == 0
            ? $"No kingdom named '{query}' was found."
            : $"Kingdom name '{query}' is ambiguous: {FormatKingdomMatches(matches)}.";
        return false;
    }

    private static bool TryFindClan(string query, out Clan clan, out string error)
    {
        List<Clan> matches = FindExactMatches(
            Clan.All,
            query,
            candidate => candidate.Name.ToString(),
            candidate => candidate.StringId);
        if (matches.Count == 1)
        {
            clan = matches[0];
            error = string.Empty;
            return true;
        }

        clan = null!;
        error = matches.Count == 0
            ? $"No clan named '{query}' was found."
            : $"Clan name '{query}' is ambiguous: {FormatClanMatches(matches)}.";
        return false;
    }

    private static List<T> FindExactMatches<T>(
        IEnumerable<T> candidates,
        string query,
        Func<T, string> getName,
        Func<T, string> getId)
    {
        string normalizedQuery = NormalizeQuery(query);
        List<T> nameMatches = candidates
            .Where(candidate => string.Equals(
                getName(candidate),
                normalizedQuery,
                StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        if (nameMatches.Count > 0)
        {
            return nameMatches;
        }

        return candidates
            .Where(candidate => string.Equals(
                getId(candidate),
                normalizedQuery,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool TrySplitPair(
        IReadOnlyList<string> arguments,
        out string first,
        out string second)
    {
        int separatorIndex = -1;
        for (int index = 0; index < arguments.Count; index++)
        {
            if (arguments[index] == PairSeparator)
            {
                if (separatorIndex >= 0)
                {
                    first = string.Empty;
                    second = string.Empty;
                    return false;
                }

                separatorIndex = index;
            }
        }

        if (separatorIndex < 0)
        {
            if (arguments.Count != 2)
            {
                first = string.Empty;
                second = string.Empty;
                return false;
            }

            first = NormalizeQuery(arguments[0]);
            second = NormalizeQuery(arguments[1]);
            return first.Length > 0 && second.Length > 0;
        }

        first = JoinArguments(arguments.Take(separatorIndex));
        second = JoinArguments(arguments.Skip(separatorIndex + 1));
        return first.Length > 0 && second.Length > 0;
    }

    private static string JoinArguments(IEnumerable<string> arguments)
    {
        return NormalizeQuery(string.Join(" ", arguments));
    }

    private static string NormalizeQuery(string query)
    {
        return query.Trim().Trim('"');
    }

    private static string FormatKingdomMatches(IEnumerable<Kingdom> matches)
    {
        return string.Join(
            ", ",
            matches.Select(match => $"{match.Name} ({match.StringId})"));
    }

    private static string FormatClanMatches(IEnumerable<Clan> matches)
    {
        return string.Join(
            ", ",
            matches.Select(match => $"{match.Name} ({match.StringId})"));
    }

    private static string FormatContractEnd(CampaignTime endTime)
    {
        if (endTime == CampaignTime.Zero || endTime == CampaignTime.Never)
        {
            return "indefinite";
        }

        float days = endTime.IsPast
            ? -endTime.ElapsedDaysUntilNow
            : endTime.RemainingDaysFromNow;
        return days.ToString("0.##", CultureInfo.InvariantCulture) + "_days";
    }
}

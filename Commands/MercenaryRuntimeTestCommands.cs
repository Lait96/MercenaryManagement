using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace MercenaryManagement.Commands;

public static class MercenaryRuntimeTestCommands
{
    private enum PlayerRole
    {
        Free,
        Vassal,
        Mercenary,
        Ruler
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_runtime_test", "campaign")]
    public static string Run(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count != 2
            || !TryParseRole(arguments[0], out PlayerRole expectedRole)
            || !string.Equals(arguments[1], "confirm", StringComparison.OrdinalIgnoreCase))
        {
            return "Usage: campaign.mm_runtime_test <free|vassal|mercenary|ruler> confirm\n"
                   + "Run only on one of the four disposable runtime-test saves; the test performs reversible-looking but persistent campaign actions.";
        }

        Clan? playerClan = Clan.PlayerClan;
        Hero? mainHero = Hero.MainHero;
        if (playerClan is null || mainHero is null)
            return "RUNTIME TEST ABORTED: player clan or main hero is unavailable.";

        PlayerRole actualRole = GetPlayerRole(playerClan);
        var report = new RuntimeTestReport(expectedRole, actualRole);
        report.Pass("campaign context", "active Campaign, player clan and main hero are available");
        if (actualRole != expectedRole)
        {
            report.Fail("save role",
                $"expected={expectedRole}; actual={actualRole}. No mutating checks were started.");
            return report.Build();
        }
        report.Pass("save role", actualRole.ToString());

        RunCommonChecks(report, playerClan, mainHero);
        switch (expectedRole)
        {
            case PlayerRole.Free:
                RunFreeChecks(report, playerClan);
                break;
            case PlayerRole.Vassal:
                RunVassalChecks(report, playerClan);
                break;
            case PlayerRole.Mercenary:
                RunMercenaryChecks(report, playerClan);
                break;
            case PlayerRole.Ruler:
                RunRulerChecks(report, playerClan);
                break;
        }
        return report.Build();
    }

    private static void RunCommonChecks(
        RuntimeTestReport report,
        Clan playerClan,
        Hero mainHero)
    {
        report.Check("player identity", () =>
            playerClan.Leader == mainHero
                ? "player clan leader matches Hero.MainHero"
                : throw new InvalidOperationException("player clan leader does not match Hero.MainHero"));

        MercenaryContractBehavior? contracts = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        MercenaryOfferBehavior? offers = Campaign.Current?
            .GetCampaignBehavior<MercenaryOfferBehavior>();
        report.Check("campaign behaviors", () =>
            contracts is not null && offers is not null
                ? "contract and offer behaviors are registered"
                : throw new InvalidOperationException("required campaign behavior is missing"));

        report.Check("null army callbacks", () =>
        {
            MercenaryKingdomAiBehavior.RunDiagnosticNullArmyCallbacks();
            return "ArmyCreated(null) and ArmyDispersed(null) completed without exception";
        });

        report.Check("live army lifecycle probes", () =>
        {
            int armies = 0;
            int missingLeaders = 0;
            foreach (Kingdom kingdom in Kingdom.All)
            {
                foreach (Army army in kingdom.Armies.ToArray())
                {
                    armies++;
                    if (army.LeaderParty is null) missingLeaders++;
                    MercenaryKingdomAiBehavior.RunDiagnosticArmyProbe(army, false);
                    MercenaryKingdomAiBehavior.RunDiagnosticArmyProbe(army, true);
                }
            }
            return $"probed={armies}; armies_without_leader={missingLeaders}";
        });

        report.Check("live party state probes", () =>
        {
            int parties = 0;
            int armyParties = 0;
            int missingArmyLeaders = 0;
            foreach (Clan clan in Clan.All)
            foreach (var component in clan.WarPartyComponents.ToArray())
            {
                MobileParty? party = component.MobileParty;
                if (party is null) continue;
                parties++;
                MercenaryPriorityBehavior.GetPassiveContributionEvidence(party);
                if (party.Army is null) continue;
                armyParties++;
                if (party.Army.LeaderParty is null) missingArmyLeaders++;
            }
            return $"probed={parties}; in_armies={armyParties}; missing_army_leaders={missingArmyLeaders}";
        });

        report.Check("Harmony runtime patches", () =>
        {
            int patchedMethods = Harmony.GetAllPatchedMethods().Count(method =>
                Harmony.GetPatchInfo(method)?.Owners.Any(owner =>
                    owner.StartsWith("MercenaryManagement", StringComparison.Ordinal)) == true);
            if (patchedMethods < 4)
                throw new InvalidOperationException($"expected at least 4 patched methods; found={patchedMethods}");
            return $"applied MercenaryManagement targets={patchedMethods}";
        });

        report.Check("active contract integrity", () =>
        {
            int activeContracts = 0;
            foreach (Clan clan in Clan.All.Where(item => item.IsUnderMercenaryService))
            {
                if (contracts?.GetOrCreateContract(clan, out MercenaryContract contract) != true)
                    throw new InvalidOperationException($"no contract for serving clan {clan.StringId}");
                activeContracts++;
                if (contract.Clan != clan || contract.EmployerKingdom != clan.Kingdom)
                    throw new InvalidOperationException($"participant mismatch for {clan.StringId}");
                AssertRiskIsFinite(contract);
            }
            return $"validated={activeContracts}";
        });
    }

    private static void RunFreeChecks(RuntimeTestReport report, Clan playerClan)
    {
        report.Check("free role invariants", () =>
        {
            if (playerClan.Kingdom is not null || playerClan.IsUnderMercenaryService
                || playerClan.Fiefs.Count > 0)
                throw new InvalidOperationException("free test save must have no kingdom, service, or fiefs");
            return "independent clan is eligible to offer services";
        });

        report.Check("Offer Services full transition", () =>
        {
            MercenaryOfferBehavior offers = RequireBehavior<MercenaryOfferBehavior>();
            Kingdom? kingdom = offers.GetServiceRequestKingdoms(playerClan).FirstOrDefault();
            if (kingdom is null)
                throw new InvalidOperationException("no eligible kingdom; prepare the save with at least one available kingdom");
            bool joined = false;
            try
            {
                MercenaryOffer? offer = offers.RequestOffer(playerClan, kingdom);
                if (offer is null) throw new InvalidOperationException("eligible kingdom refused RequestOffer");
                if (!offers.GetSavedOffers().Any(item => ReferenceEquals(item, offer)))
                    throw new InvalidOperationException("created offer was not persisted by the behavior");
                bool accepted = offers.TryAccept(offer);
                joined = playerClan.IsUnderMercenaryService;
                if (!accepted)
                    throw new InvalidOperationException("TryAccept rejected a freshly created valid offer");
                if (!joined || playerClan.Kingdom != kingdom)
                    throw new InvalidOperationException("native join transition did not enter mercenary service");
                MercenaryContractBehavior contracts = RequireBehavior<MercenaryContractBehavior>();
                if (!contracts.GetOrCreateContract(playerClan, out MercenaryContract contract))
                    throw new InvalidOperationException("accepted offer did not create an active contract");
                AssertRiskIsFinite(contract);
                MercenaryDismissalService.EndService(playerClan);
                joined = playerClan.IsUnderMercenaryService;
                if (joined || playerClan.Kingdom is not null)
                    throw new InvalidOperationException("cleanup did not restore the free-clan role");
                return $"requested, accepted and completed service with {kingdom.StringId}";
            }
            finally
            {
                if (joined || playerClan.IsUnderMercenaryService)
                    MercenaryDismissalService.EndService(playerClan);
            }
        });
    }

    private static void RunVassalChecks(RuntimeTestReport report, Clan playerClan)
    {
        report.Check("vassal role invariants", () =>
        {
            Kingdom? kingdom = playerClan.Kingdom;
            if (kingdom is null || playerClan.IsUnderMercenaryService
                || MercenaryHiringService.IsPlayerRuler(kingdom))
                throw new InvalidOperationException("save is not a non-ruler vassal state");
            return $"vassal of {kingdom.StringId}";
        });
        report.Check("vassal Offer Services denial", () =>
        {
            MercenaryOfferBehavior offers = RequireBehavior<MercenaryOfferBehavior>();
            if (offers.GetServiceRequestKingdoms(playerClan).Any())
                throw new InvalidOperationException("vassal received service-request kingdoms");
            return "no service-request kingdoms exposed";
        });
        report.Check("vassal hiring denial", () =>
        {
            Kingdom kingdom = playerClan.Kingdom!;
            Clan? candidate = Clan.All.FirstOrDefault(MercenaryHiringService.IsFreeMercenaryClan);
            if (candidate is null)
                throw new InvalidOperationException("no free mercenary clan exists in the test save");
            if (!MercenaryHiringService.TryGetFailureReason(candidate, kingdom, out _))
                throw new InvalidOperationException("non-ruler vassal passed ruler-only hiring validation");
            return $"hiring correctly denied for candidate={candidate.StringId}";
        });
    }

    private static void RunMercenaryChecks(RuntimeTestReport report, Clan playerClan)
    {
        report.Check("mercenary role invariants", () =>
        {
            if (!playerClan.IsUnderMercenaryService || playerClan.Kingdom is null)
                throw new InvalidOperationException("player is not under active mercenary service");
            return $"serving={playerClan.Kingdom.StringId}";
        });
        report.Check("mercenary contract tick", () =>
        {
            MercenaryContractBehavior contracts = RequireBehavior<MercenaryContractBehavior>();
            if (!contracts.GetOrCreateContract(playerClan, out MercenaryContract contract))
                throw new InvalidOperationException("active player contract is missing");
            if (!contracts.RunDiagnosticDailyEvaluation(playerClan, 1))
                throw new InvalidOperationException("diagnostic daily evaluation was rejected");
            if (!IsPercentage(contract.Satisfaction)
                || !IsPercentage(contract.Reliability)
                || !IsPercentage(contract.EmployerSatisfaction))
                throw new InvalidOperationException("daily evaluation produced invalid percentages");
            AssertRiskIsFinite(contract);
            return $"evaluation completed; departure={MercenaryEvaluationService.CalculateDepartureRisk(contract):0.##}; dismissal={MercenaryEvaluationService.CalculateEmployerTerminationRisk(contract):0.##}";
        });
        report.Check("mercenary Offer Services denial", () =>
        {
            if (RequireBehavior<MercenaryOfferBehavior>()
                .GetServiceRequestKingdoms(playerClan).Any())
                throw new InvalidOperationException("serving mercenary received service-request kingdoms");
            return "Offer Services is unavailable while contracted";
        });
    }

    private static void RunRulerChecks(RuntimeTestReport report, Clan playerClan)
    {
        report.Check("ruler role invariants", () =>
        {
            Kingdom? kingdom = playerClan.Kingdom;
            if (kingdom is null || !MercenaryHiringService.IsPlayerRuler(kingdom))
                throw new InvalidOperationException("player is not the active kingdom ruler");
            return $"rules={kingdom.StringId}";
        });
        report.Check("ruler hire-contract-dismiss transition", () =>
        {
            Kingdom kingdom = playerClan.Kingdom!;
            Clan? candidate = Clan.All.FirstOrDefault(clan =>
                !MercenaryHiringService.TryGetFailureReason(clan, kingdom, out _));
            if (candidate is null)
                throw new InvalidOperationException("no currently hireable mercenary clan; ensure gold, relations and a free clan are available");
            bool joined = false;
            try
            {
                MercenaryHiringService.Hire(candidate, kingdom);
                joined = candidate.IsUnderMercenaryService;
                if (!joined || candidate.Kingdom != kingdom)
                    throw new InvalidOperationException("native hire action did not join the candidate");
                MercenaryContractBehavior contracts = RequireBehavior<MercenaryContractBehavior>();
                if (!contracts.GetOrCreateContract(candidate, out MercenaryContract contract))
                    throw new InvalidOperationException("hired clan did not receive a contract");
                if (!contracts.SetStrategicPriority(candidate, MercenaryStrategicPriority.JoinArmies))
                    throw new InvalidOperationException("ruler could not assign Join Armies priority");
                if (!contracts.RunDiagnosticDailyEvaluation(candidate, 1))
                    throw new InvalidOperationException("hired clan daily evaluation failed");
                AssertRiskIsFinite(contract);
                MercenaryDismissalService.EndService(candidate);
                joined = candidate.IsUnderMercenaryService;
                if (joined || candidate.Kingdom is not null)
                    throw new InvalidOperationException("dismissed test clan did not return to free state");
                return $"hired, evaluated and dismissed clan={candidate.StringId}";
            }
            finally
            {
                if (joined || candidate.IsUnderMercenaryService)
                    MercenaryDismissalService.EndService(candidate);
            }
        });
        report.Check("ruler Offer Services denial", () =>
        {
            if (RequireBehavior<MercenaryOfferBehavior>()
                .GetServiceRequestKingdoms(playerClan).Any())
                throw new InvalidOperationException("ruler received service-request kingdoms");
            return "Offer Services is unavailable to rulers";
        });
    }

    private static T RequireBehavior<T>() where T : CampaignBehaviorBase
    {
        return Campaign.Current?.GetCampaignBehavior<T>()
               ?? throw new InvalidOperationException($"{typeof(T).Name} is not registered");
    }

    private static void AssertRiskIsFinite(MercenaryContract contract)
    {
        RiskBreakdown departure = MercenaryEvaluationService.GetDepartureRiskBreakdown(contract);
        RiskBreakdown dismissal = MercenaryEvaluationService.GetEmployerTerminationRiskBreakdown(contract);
        if (!IsPercentage(departure.Total) || !IsPercentage(dismissal.Total))
            throw new InvalidOperationException("contract risk is not a finite percentage");
    }

    private static bool IsPercentage(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 100f;

    private static PlayerRole GetPlayerRole(Clan playerClan)
    {
        if (playerClan.IsUnderMercenaryService) return PlayerRole.Mercenary;
        if (playerClan.Kingdom is null) return PlayerRole.Free;
        return MercenaryHiringService.IsPlayerRuler(playerClan.Kingdom)
            ? PlayerRole.Ruler
            : PlayerRole.Vassal;
    }

    private static bool TryParseRole(string value, out PlayerRole role)
    {
        switch (value.ToLowerInvariant())
        {
            case "free": role = PlayerRole.Free; return true;
            case "vassal": role = PlayerRole.Vassal; return true;
            case "mercenary": role = PlayerRole.Mercenary; return true;
            case "ruler": role = PlayerRole.Ruler; return true;
            default: role = default; return false;
        }
    }

    private sealed class RuntimeTestReport
    {
        private readonly StringBuilder _lines = new StringBuilder();
        private int _passed;
        private int _failed;

        public RuntimeTestReport(PlayerRole expected, PlayerRole actual)
        {
            _lines.AppendLine("MERCENARY MANAGEMENT RUNTIME TEST")
                .Append("expected_role=").Append(expected)
                .Append("; actual_role=").Append(actual).AppendLine();
        }

        public void Check(string name, Func<string> test)
        {
            try
            {
                Pass(name, test());
            }
            catch (Exception exception)
            {
                Fail(name, $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        public void Pass(string name, string detail)
        {
            _passed++;
            _lines.Append("PASS | ").Append(name).Append(" | ").AppendLine(detail);
        }

        public void Fail(string name, string detail)
        {
            _failed++;
            _lines.Append("FAIL | ").Append(name).Append(" | ").AppendLine(detail);
        }

        public string Build()
        {
            _lines.Append("SUMMARY | pass=").Append(_passed)
                .Append("; fail=").Append(_failed)
                .Append("; result=").Append(_failed == 0 ? "PASS" : "FAIL")
                .AppendLine()
                .Append("IMPORTANT | discard this test save after the run; do not overwrite the source copy.");
            return _lines.ToString();
        }
    }
}

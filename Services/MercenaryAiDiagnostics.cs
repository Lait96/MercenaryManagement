using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;

namespace MercenaryManagement.Services;

internal static class MercenaryAiDiagnostics
{
    private static readonly Dictionary<string, Aggregate> Aggregates =
        new Dictionary<string, Aggregate>(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> EventCounts =
        new Dictionary<string, int>(StringComparer.Ordinal);
    private static string? _trackedClanId;

    public static bool IsEnabled => _trackedClanId is not null;

    public static void Start(Clan clan)
    {
        _trackedClanId = clan.StringId;
        Aggregates.Clear();
        EventCounts.Clear();
    }

    public static void Stop()
    {
        _trackedClanId = null;
        Aggregates.Clear();
        EventCounts.Clear();
    }

    public static void Clear()
    {
        Aggregates.Clear();
        EventCounts.Clear();
    }

    public static void Record(
        Clan? clan,
        MercenaryStrategicPriority priority,
        string decision,
        float nativeWeight,
        float finalWeight)
    {
        if (clan is null || clan.StringId != _trackedClanId)
        {
            return;
        }

        string key = priority + "|" + decision;
        if (!Aggregates.TryGetValue(key, out Aggregate aggregate))
        {
            aggregate = new Aggregate(priority, decision);
            Aggregates.Add(key, aggregate);
        }

        aggregate.Add(nativeWeight, finalWeight);
    }

    public static void RecordEvent(
        Clan? clan,
        MercenaryStrategicPriority priority,
        string eventName)
    {
        if (clan is null || clan.StringId != _trackedClanId)
        {
            return;
        }

        string key = priority + "|" + eventName;
        EventCounts.TryGetValue(key, out int count);
        EventCounts[key] = count + 1;
    }

    public static string GetReport()
    {
        if (_trackedClanId is null)
        {
            return "AI diagnostics are disabled.";
        }

        var result = new StringBuilder("AI diagnostics for clan_id=")
            .Append(_trackedClanId);
        if (Aggregates.Count == 0 && EventCounts.Count == 0)
        {
            return result.AppendLine().Append("No decisions have been recorded yet.").ToString();
        }

        foreach (Aggregate aggregate in Aggregates.Values
                     .OrderBy(value => value.Priority)
                     .ThenBy(value => value.Decision, StringComparer.Ordinal))
        {
            result.AppendLine()
                .Append("priority=").Append(aggregate.Priority)
                .Append(" | decision=").Append(aggregate.Decision)
                .Append(" | count=").Append(aggregate.Count)
                .Append(" | avg_native=").Append(Format(aggregate.NativeTotal / aggregate.Count))
                .Append(" | avg_final=").Append(Format(aggregate.FinalTotal / aggregate.Count))
                .Append(" | avg_multiplier=").Append(aggregate.GetAverageMultiplierText())
                .Append(" | changed=").Append(aggregate.ChangedCount);
        }

        foreach (KeyValuePair<string, int> eventCount in EventCounts
                     .OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            string[] keyParts = eventCount.Key.Split('|');
            result.AppendLine()
                .Append("priority=").Append(keyParts[0])
                .Append(" | event=").Append(keyParts[1])
                .Append(" | count=").Append(eventCount.Value);
        }

        return result.ToString();
    }

    private static string Format(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private sealed class Aggregate
    {
        public Aggregate(MercenaryStrategicPriority priority, string decision)
        {
            Priority = priority;
            Decision = decision;
        }

        public MercenaryStrategicPriority Priority { get; }
        public string Decision { get; }
        public int Count { get; private set; }
        public int ChangedCount { get; private set; }
        public double NativeTotal { get; private set; }
        public double FinalTotal { get; private set; }
        private double MultiplierTotal { get; set; }
        private int MultiplierCount { get; set; }

        public void Add(float nativeWeight, float finalWeight)
        {
            Count++;
            NativeTotal += nativeWeight;
            FinalTotal += finalWeight;
            if (nativeWeight != finalWeight)
            {
                ChangedCount++;
            }

            if (Math.Abs(nativeWeight) > 0.0001f)
            {
                MultiplierTotal += finalWeight / nativeWeight;
                MultiplierCount++;
            }
        }

        public string GetAverageMultiplierText()
        {
            return MultiplierCount > 0
                ? Format(MultiplierTotal / MultiplierCount)
                : "n/a";
        }
    }
}

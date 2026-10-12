using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace MercenaryManagement.Services;

// How many mercenary clans one kingdom may employ at once. The default stays at three;
// players can change it in ModuleData/MercenaryManagementSettings.xml, read once when the
// module loads. A missing file, a missing or unreadable value keeps the default; a value
// out of range is clamped.
internal static class MercenaryHiringLimitPolicy
{
    public const int DefaultMaximumHiredMercenaryClans = 3;
    public const int MinimumMaximumHiredMercenaryClans = 1;
    public const int UpperMaximumHiredMercenaryClans = 20;
    public const string SettingsFileName = "MercenaryManagementSettings.xml";

    public static int MaximumHiredMercenaryClans { get; private set; } = DefaultMaximumHiredMercenaryClans;

    public static int Normalize(int value)
    {
        return Math.Max(MinimumMaximumHiredMercenaryClans, Math.Min(UpperMaximumHiredMercenaryClans, value));
    }

    public static bool CanHireAnother(int hiredMercenaryClans, int maximumHiredMercenaryClans)
    {
        return hiredMercenaryClans < Normalize(maximumHiredMercenaryClans);
    }

    // <MercenaryManagementSettings><MaximumHiredMercenaryClans value="3" /></MercenaryManagementSettings>
    public static int ParseMaximumHiredMercenaryClans(string? settingsXml)
    {
        if (string.IsNullOrWhiteSpace(settingsXml))
        {
            return DefaultMaximumHiredMercenaryClans;
        }

        try
        {
            XElement? element = XDocument.Parse(settingsXml).Root?.Element("MaximumHiredMercenaryClans");
            string? text = element?.Attribute("value")?.Value ?? element?.Value;
            return int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                ? Normalize(value)
                : DefaultMaximumHiredMercenaryClans;
        }
        catch (Exception)
        {
            return DefaultMaximumHiredMercenaryClans;
        }
    }

    // moduleDirectory: the module's root folder (the one holding SubModule.xml).
    public static void Load(string? moduleDirectory)
    {
        MaximumHiredMercenaryClans = DefaultMaximumHiredMercenaryClans;
        if (string.IsNullOrEmpty(moduleDirectory))
        {
            return;
        }

        try
        {
            string path = Path.Combine(moduleDirectory, "ModuleData", SettingsFileName);
            if (File.Exists(path))
            {
                MaximumHiredMercenaryClans = ParseMaximumHiredMercenaryClans(File.ReadAllText(path));
            }
        }
        catch (Exception)
        {
            MaximumHiredMercenaryClans = DefaultMaximumHiredMercenaryClans;
        }
    }
}

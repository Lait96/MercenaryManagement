using MercenaryManagement.Contracts;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal static class MercenaryPriorityText
{
    public static TextObject GetName(MercenaryStrategicPriority priority)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.JoinArmies:
                return new TextObject("{=MercenaryManagement_PriorityJoinArmies}Join armies");
            case MercenaryStrategicPriority.EconomicWarfare:
                return new TextObject("{=MercenaryManagement_PriorityEconomicWarfare}Economic warfare");
            case MercenaryStrategicPriority.TerritorialDefense:
                return new TextObject("{=MercenaryManagement_PriorityTerritorialDefense}Territorial defense");
            default:
                return new TextObject("{=MercenaryManagement_PriorityIndependentOperations}Independent operations");
        }
    }

    public static TextObject GetDescription(MercenaryStrategicPriority priority)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.JoinArmies:
                return new TextObject("{=MercenaryManagement_PriorityJoinArmiesDescription}The clan is expected to join and support the kingdom's armies.");
            case MercenaryStrategicPriority.EconomicWarfare:
                return new TextObject("{=MercenaryManagement_PriorityEconomicWarfareDescription}The clan is expected to raid enemy villages and attack caravans and villager parties.");
            case MercenaryStrategicPriority.TerritorialDefense:
                return new TextObject("{=MercenaryManagement_PriorityTerritorialDefenseDescription}The clan is expected to defend friendly lands and respond to local threats.");
            default:
                return new TextObject("{=MercenaryManagement_PriorityIndependentOperationsDescription}The clan chooses its own actions according to the current situation.");
        }
    }
}

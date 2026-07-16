using System.Collections.Generic;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MercenaryManagement.SaveSystem;

public sealed class MercenarySaveableTypeDefiner : SaveableTypeDefiner
{
    private const int SaveBaseId = 1_947_031;

    public MercenarySaveableTypeDefiner()
        : base(SaveBaseId)
    {
    }

    protected override void DefineClassTypes()
    {
        AddClassDefinition(typeof(MercenaryContract), 1);
    }

    protected override void DefineEnumTypes()
    {
        AddEnumDefinition(typeof(MercenaryContractState), 2);
        AddEnumDefinition(typeof(MercenaryStrategicPriority), 3);
    }

    protected override void DefineContainerDefinitions()
    {
        ConstructContainerDefinition(typeof(Dictionary<string, MercenaryContract>));
        ConstructContainerDefinition(typeof(Dictionary<string, CampaignTime>));
    }
}

using System.Collections.Generic;
using MercenaryManagement.Contracts;
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
    }

    protected override void DefineContainerDefinitions()
    {
        ConstructContainerDefinition(typeof(Dictionary<string, MercenaryContract>));
    }
}

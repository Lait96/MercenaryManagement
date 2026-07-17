using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using MercenaryManagement.Patches;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Library;

namespace MercenaryManagement.UI;

[ViewModelMixin(true)]
internal sealed class ClanManagementVMMixin : BaseViewModelMixin<ClanManagementVM>
{
    private readonly PlayerMercenaryContractVM _mercenaryContract = new PlayerMercenaryContractVM();
    private bool _isMercenaryContractSelected;
    private string _mercenaryContractText = string.Empty;

    public ClanManagementVMMixin(ClanManagementVM viewModel)
        : base(viewModel)
    {
        ClanManagementSelectionPatch.SelectedCategoryChanged += OnSelectedCategoryChanged;
        RefreshValues();
    }

    [DataSourceProperty]
    public PlayerMercenaryContractVM MercenaryContract => _mercenaryContract;

    [DataSourceProperty]
    public bool IsMercenaryContractTabVisible =>
        Clan.PlayerClan is not null && Clan.PlayerClan.Leader == Hero.MainHero;

    [DataSourceProperty]
    public bool IsMercenaryContractSelected
    {
        get => _isMercenaryContractSelected;
        private set
        {
            if (_isMercenaryContractSelected == value)
            {
                return;
            }

            _isMercenaryContractSelected = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string MercenaryContractText
    {
        get => _mercenaryContractText;
        private set
        {
            if (_mercenaryContractText == value) return;
            _mercenaryContractText = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceMethod]
    public void ExecuteShowMercenaryContract()
    {
        if (!IsMercenaryContractTabVisible || ViewModel is null)
        {
            return;
        }

        ViewModel.IsMembersSelected = false;
        ViewModel.IsPartiesSelected = false;
        ViewModel.IsFiefsSelected = false;
        ViewModel.IsIncomeSelected = false;
        ViewModel.ClanMembers.IsSelected = false;
        ViewModel.ClanParties.IsSelected = false;
        ViewModel.ClanFiefs.IsSelected = false;
        ViewModel.ClanIncome.IsSelected = false;
        MercenaryContract.Refresh();
        IsMercenaryContractSelected = true;
    }

    private void RefreshValues()
    {
        MercenaryContractText = new TaleWorlds.Localization.TextObject(
            "{=MercenaryManagement_PlayerMercenaryTab}Mercenary").ToString();
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        RefreshValues();
        OnPropertyChangedWithValue(IsMercenaryContractTabVisible, nameof(IsMercenaryContractTabVisible));
        OnPropertyChangedWithValue(MercenaryContractText, nameof(MercenaryContractText));

        if (!IsMercenaryContractTabVisible && IsMercenaryContractSelected)
        {
            IsMercenaryContractSelected = false;
            ViewModel?.SetSelectedCategory(0);
        }

        MercenaryContract.RefreshValues();
    }

    public override void OnFinalize()
    {
        ClanManagementSelectionPatch.SelectedCategoryChanged -= OnSelectedCategoryChanged;
        MercenaryContract.OnFinalize();
        base.OnFinalize();
    }

    private void OnSelectedCategoryChanged(ClanManagementVM viewModel, int index)
    {
        if (viewModel != ViewModel) return;
        if (index == 4)
        {
            ExecuteShowMercenaryContract();
        }
        else
        {
            IsMercenaryContractSelected = false;
        }
    }
}

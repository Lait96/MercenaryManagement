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

    public ClanManagementVMMixin(ClanManagementVM viewModel)
        : base(viewModel)
    {
        ClanManagementSelectionPatch.SelectedCategoryChanged += OnSelectedCategoryChanged;
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
    public string MercenaryContractText => new TaleWorlds.Localization.TextObject(
        "{=MercenaryManagement_PlayerMercenaryTab}Mercenary").ToString();

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

    public override void OnRefresh()
    {
        base.OnRefresh();
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

    private void OnSelectedCategoryChanged(ClanManagementVM viewModel)
    {
        if (viewModel == ViewModel)
        {
            IsMercenaryContractSelected = false;
        }
    }
}

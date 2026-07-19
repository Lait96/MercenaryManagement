using System.ComponentModel;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
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
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
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
        if (ViewModel is not null)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        MercenaryContract.OnFinalize();
        base.OnFinalize();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (IsMercenaryContractSelected
            && args.PropertyName is nameof(ClanManagementVM.IsMembersSelected)
                or nameof(ClanManagementVM.IsPartiesSelected)
                or nameof(ClanManagementVM.IsFiefsSelected)
                or nameof(ClanManagementVM.IsIncomeSelected)
            && ViewModel is { } viewModel
            && (viewModel.IsMembersSelected
                || viewModel.IsPartiesSelected
                || viewModel.IsFiefsSelected
                || viewModel.IsIncomeSelected))
        {
            IsMercenaryContractSelected = false;
        }
    }
}

using System;
using System.ComponentModel;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

[ViewModelMixin(true)]
internal sealed class KingdomManagementVmMixin : BaseViewModelMixin<KingdomManagementVM>
{
    private readonly KingdomCategoryVM[] _categories;
    private readonly MercenaryClansVM _mercenaries = new MercenaryClansVM();
    private bool _isMercenariesSelected;
    private string _mercenariesText = string.Empty;

    public KingdomManagementVmMixin(KingdomManagementVM viewModel)
        : base(viewModel)
    {
        _categories =
        [
            viewModel.Clan,
            viewModel.Settlement,
            viewModel.Policy,
            viewModel.Army,
            viewModel.Diplomacy
        ];

        foreach (KingdomCategoryVM category in _categories)
        {
            category.PropertyChanged += OnCategoryPropertyChanged;
        }

        RefreshValues();
    }

    [DataSourceProperty]
    public MercenaryClansVM Mercenaries => _mercenaries;

    [DataSourceProperty]
    public bool IsMercenariesSelected
    {
        get => _isMercenariesSelected;
        private set
        {
            if (_isMercenariesSelected == value)
            {
                return;
            }

            _isMercenariesSelected = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string MercenariesText
    {
        get => _mercenariesText;
        private set
        {
            if (_mercenariesText == value)
            {
                return;
            }

            _mercenariesText = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceMethod]
    public void ExecuteShowMercenaries()
    {
        Mercenaries.Refresh(ViewModel?.Kingdom);

        foreach (KingdomCategoryVM category in _categories)
        {
            category.Show = false;
        }

        IsMercenariesSelected = true;
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        RefreshValues();
        Mercenaries.RefreshValues();

        if (IsMercenariesSelected)
        {
            Mercenaries.Refresh(ViewModel?.Kingdom);
        }
    }

    public override void OnFinalize()
    {
        foreach (KingdomCategoryVM category in _categories)
        {
            category.PropertyChanged -= OnCategoryPropertyChanged;
        }

        Mercenaries.OnFinalize();
        base.OnFinalize();
    }

    private void RefreshValues()
    {
        MercenariesText = new TextObject("{=MercenaryManagement_Mercenaries}Mercenaries").ToString();
    }

    private void OnCategoryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(KingdomCategoryVM.Show)
            && sender is KingdomCategoryVM { Show: true })
        {
            IsMercenariesSelected = false;
        }
    }
}

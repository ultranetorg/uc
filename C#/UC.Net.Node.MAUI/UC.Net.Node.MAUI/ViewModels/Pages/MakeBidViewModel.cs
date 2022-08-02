namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class MakeBidViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public MakeBidViewModel(ILogger<MakeBidViewModel> logger) : base(logger)
    {
    }
}

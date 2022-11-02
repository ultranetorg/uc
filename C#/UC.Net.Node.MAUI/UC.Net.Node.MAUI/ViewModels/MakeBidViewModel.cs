namespace UC.Net.Node.MAUI.ViewModels;

public partial class MakeBidViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public MakeBidViewModel(ILogger<MakeBidViewModel> logger) : base(logger)
    {
    }
}

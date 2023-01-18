namespace UC.Umc.ViewModels;

public partial class MakeBidViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	private string _currentBid = "185 UNT ($104 100)";

    public MakeBidViewModel(ILogger<MakeBidViewModel> logger) : base(logger)
    {
    }
}

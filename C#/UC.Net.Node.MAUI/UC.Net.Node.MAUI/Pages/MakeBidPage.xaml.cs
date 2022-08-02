namespace UC.Net.Node.MAUI.Pages;

public partial class MakeBidPage : CustomPage
{
    public MakeBidPage()
    {
        InitializeComponent();
        BindingContext = new MakeBidViewModel(ServiceHelper.GetService<ILogger<MakeBidViewModel>>());
    }
}

public partial class MakeBidViewModel : BaseAccountViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public MakeBidViewModel(ILogger<MakeBidViewModel> logger) : base(logger)
    {
    }
}

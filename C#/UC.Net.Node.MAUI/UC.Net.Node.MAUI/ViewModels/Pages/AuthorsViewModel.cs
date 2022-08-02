namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorsViewModel : BaseViewModel
{
	[ObservableProperty]
    private Author _selectedItem;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<string> _authorsFilter = new();

    public AuthorsViewModel(ILogger<AuthorsViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void Create()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }
	
	[RelayCommand]
    private async void Restore()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }
	
	[RelayCommand]
    private async void ItemTapped(Author Author)
    {
        if (Author == null) 
            return;
        await Shell.Current.Navigation.PushAsync(new AuthorSearchPage(Author));
    }
	
	[RelayCommand]
    private async void Options(Wallet wallet)
    {
		// has been changed from Author to Wallet
        await AccountOptionsPopup.Show(wallet);
    }

    private async void TransferAuthorCommand(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new AuthorRegistrationPage());
    }

    private async void MackBidCommand(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new MakeBidPage());
    }

	private void FillFakeData()
	{
        AuthorsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.None,Name= "amazon.com" , ActiveDue= "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.Higher, Name = "Auction A", ActiveDue = "43 days left", CurrentBid=2435});
        Authors.Add(new Author { BidStatus = BidStatus.Lower, Name = "Auction A", ActiveDue = "43 days left", CurrentBid = 2435 });
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "ultranet.org", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.Higher, Name = "Auction A", ActiveDue = "43 days left", CurrentBid = 2435 });
        Authors.Add(new Author { BidStatus = BidStatus.Lower, Name = "Auction B", ActiveDue = "43 days left", CurrentBid = 2435 });
	}
}

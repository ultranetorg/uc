namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class SelectAuthorViewModel : BaseViewModel
{
    public Author SelectedAuthor;

    public Page Page { get; }
        
    public SelectAuthorPopup Popup { get; }

	[ObservableProperty]
	private CustomCollection<Author> _authors = new();

    public SelectAuthorViewModel(SelectAuthorPopup popup, ILogger<SelectAuthorViewModel> logger) : base(logger)
    {
        Popup = popup;
		AddFakeData();
    }

	[RelayCommand]
    private void ItemTapped(Author Author)
    {
        SelectedAuthor = Author;
    }

	[RelayCommand]
    private void Close()
    {
        Popup.Hide();
    }

	private void AddFakeData()
	{
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.Higher, Name = "Auction A", ActiveDue = "43 days left", CurrentBid = 2435 });
        Authors.Add(new Author { BidStatus = BidStatus.Lower, Name = "Auction A", ActiveDue = "43 days left", CurrentBid = 2435 });
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "ultranet.org", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.None, Name = "amazon.com", ActiveDue = "Active due: 07/07/2022 (in 182 days)" });
        Authors.Add(new Author { BidStatus = BidStatus.Higher, Name = "Auction A", ActiveDue = "43 days left", CurrentBid = 2435 });
        Authors.Add(new Author { BidStatus = BidStatus.Lower, Name = "Auction B", ActiveDue = "43 days left", CurrentBid = 2435 });
	}
}


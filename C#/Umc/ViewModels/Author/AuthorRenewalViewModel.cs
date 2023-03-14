namespace UC.Umc.ViewModels;

public partial class AuthorRenewalViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(EstimatedDate))]
	private string _period = "1";

	public string EstimatedDate => string.IsNullOrWhiteSpace(Period)
		? string.Empty
		: DateTime.UtcNow.AddDays(int.Parse(Period)).ToShortDateString();

    public AuthorRenewalViewModel(ILogger<AuthorRenewalViewModel> logger) : base(logger)
    {
    }
}
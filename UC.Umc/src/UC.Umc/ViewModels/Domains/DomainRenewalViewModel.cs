namespace UC.Umc.ViewModels.Domains;

public partial class DomainRenewalViewModel(ILogger<DomainRenewalViewModel> logger) : BaseDomainViewModel(logger)
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(EstimatedDate))]
	private string _period = "1";

	public string EstimatedDate => string.IsNullOrWhiteSpace(Period)
		? string.Empty
		: DateTime.UtcNow.AddDays(int.Parse(Period)).ToShortDateString();
}
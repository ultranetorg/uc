namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class HelpDetailsViewModel : BaseViewModel
{
	public string HelpQuestionExample { get; set; }
	public string HelpAnswerExample1 { get; set; }
	public string HelpAnswerExample2 { get; set; }

    public HelpDetailsViewModel(ILogger<HelpDetailsViewModel> logger) : base(logger)
    {
		InitializeAsync();
    }

	private void InitializeAsync()
	{
		HelpQuestionExample = Properties.Resources.HelpQuestion1;
		HelpAnswerExample1 = Properties.Resources.HelpAnswer1;
		HelpAnswerExample2 = Properties.Resources.HelpAnswer2;
	}

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }  
}

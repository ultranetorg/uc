namespace UC.Umc.ViewModels;

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
	private bool _isPrivateKey;

	[ObservableProperty]
	private bool _isFilePath = true;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletFilePath;

    public RestoreAccountViewModel(IServicesMockData service, ILogger<RestoreAccountViewModel> logger) : base(logger)
    {
		_service = service;
    }

	[RelayCommand]
	private void ChangeKeySource()
	{
		IsPrivateKey = !IsPrivateKey;
		IsFilePath = !IsFilePath;
	}

	[RelayCommand]
    private async Task ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
	}

	[RelayCommand]
	private async Task ImportAsync()
	{
		await Shell.Current.Navigation.PopAsync();
	}

	internal async Task InitializeAsync()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);

		await Task.Delay(1);
	}
}

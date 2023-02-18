namespace UC.Umc.ViewModels;

public partial class ETHTransfer1ViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
	private bool _isPrivateKey;

	[ObservableProperty]
	private bool _isFilePath = true;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletFilePath;

	public ETHTransfer1ViewModel(IServicesMockData service, ILogger<ETHTransfer1ViewModel> logger): base(logger)
	{
		_service = service;
		LoadData();
	}
	
	private void LoadData()
	{
	}

	[RelayCommand]
	private void ChangeKeySource()
	{
		IsPrivateKey = !IsPrivateKey;
		IsFilePath = !IsFilePath;
	}
}

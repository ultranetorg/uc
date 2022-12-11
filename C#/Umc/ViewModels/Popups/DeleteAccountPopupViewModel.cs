namespace UC.Umc.ViewModels.Popups;

public partial class DeleteAccountPopupViewModel : BaseViewModel
{
	[ObservableProperty]
	public AccountViewModel _account;

	public DeleteAccountPopupViewModel(ILogger<DeleteAccountPopupViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private void Close() => ClosePopup();
}

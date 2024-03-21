namespace UC.Umc.ViewModels.Popups;

public partial class DeleteAccountPopupViewModel : BaseViewModel
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextToConfirm))]
	public AccountViewModel _account;

	public string TextToConfirm => Account?.Address[..9] ?? "0x07-47f0";

	[ObservableProperty]
	public string _textInput;

	public DeleteAccountPopupViewModel(ILogger<DeleteAccountPopupViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private void Close() => ClosePopup();
}

using UC.Umc.Models;

namespace UC.Umc.ViewModels.Popups;

public partial class DeleteAccountPopupViewModel(ILogger<DeleteAccountPopupViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextToConfirm))]
	public AccountModel _account;

	public string TextToConfirm => Account?.Address[..9] ?? "0x07-47f0";

	[ObservableProperty]
	public string _textInput;

	[RelayCommand]
	private void Close() => ClosePopup();
}

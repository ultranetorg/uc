namespace UC.Umc.ViewModels;

public partial class SendViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _source;

	[ObservableProperty]
    private AccountViewModel _recipient;

	[ObservableProperty]
    private int _position;

    public SendViewModel(ILogger<SendViewModel> logger) : base(logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Source = (AccountViewModel)query[QueryKeys.SOURCE_ACCOUNT];
            Recipient = (AccountViewModel)query[QueryKeys.RECIPIENT_ACCOUNT];
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Account: {Account}", Source ?? Recipient);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyQueryAttributes Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
        finally
        {
            FinishLoading();
        }
	}

	[RelayCommand]
    private void HidePopup() => ClosePopup();

	[RelayCommand]
    private async Task ConfirmAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
      
	[RelayCommand]
    private void Transfer()
    {
        if (Position != 1) 
		{
			Position += 1;
		} 
    }
}

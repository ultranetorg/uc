namespace UC.Umc.ViewModels;

public partial class SendViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _source;

	[ObservableProperty]
    private AccountViewModel _recipient;

	[ObservableProperty]
    private int _position = 0;

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
    private async Task CancelAsync()
	{
        try
        {
			if(Position == 1)
			{
				Position = 0;
			}
			else
			{
				await Navigation.PopAsync();
			}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelAsync Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
	}

	[RelayCommand]
    private async Task ConfirmAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
      
	[RelayCommand]
    private void Transfer()
    {
        if (Position == 0) 
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
    }
    
	[RelayCommand]
    private async Task SourceTapped()
    {
        await SourceAccountPopup.Show();
    }

	[RelayCommand]
    private async Task RecipientTapped()
    {
        await RecipientAccountPopup.Show();
    }
}

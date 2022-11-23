using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class SendViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountViewModel _source;

	[ObservableProperty]
	private AccountViewModel _recipient;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(FirstStep))]
	[NotifyPropertyChangedFor(nameof(SecondStep))]
	private int _position = 0;

	[ObservableProperty]
	[NotifyDataErrorInfo]
	[Required(ErrorMessage = "Required")]
	[Range(1.0, int.MaxValue, ErrorMessage = "Wrong Amount")]
	[NotifyPropertyChangedFor(nameof(AmountError))]
	[NotifyPropertyChangedFor(nameof(Comission))]
	private string _amount;

	public decimal Comission 
	{
		get
		{
			decimal.TryParse(Amount, out decimal comission);
			return comission > 0 ? comission / 100 : 0;
		}
	}

    public string AmountError => GetControlErrorMessage(nameof(Amount));

	public bool FirstStep => Position == 0;

	public bool SecondStep => Position == 1;

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

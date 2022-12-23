namespace UC.Umc.ViewModels;

public partial class AuthorRegistrationViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
    private AccountColor _selectedAccountColor;

	[ObservableProperty]
    private int _position;

    public AuthorRegistrationViewModel(ILogger<AuthorRegistrationViewModel> logger) : base(logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Author = (AuthorViewModel)query[QueryKeys.AUTHOR];
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Author: {Author}", Author);
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
    protected void Next()
    {
        if (Position < 1)
		{
			Position += 1;
		}
    }

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}

	[RelayCommand]
    private async Task ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}

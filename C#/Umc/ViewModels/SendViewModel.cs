﻿namespace UC.Umc.ViewModels;

public partial class SendViewModel : BaseViewModel
{
	[ObservableProperty]
    private int _position;

    public SendViewModel(ILogger<SendViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }

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
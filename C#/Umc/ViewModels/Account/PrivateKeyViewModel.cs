﻿namespace UC.Umc.ViewModels;

public partial class PrivateKeyViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();
	
	[ObservableProperty]
    private AccountViewModel _account;

    public PrivateKeyViewModel(ILogger<PrivateKeyViewModel> logger) : base(logger)
    { 
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Account);
    }
}
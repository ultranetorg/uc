﻿namespace UC.Umc.ViewModels;

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

    public RestoreAccountViewModel(IServicesMockData service, ILogger<RestoreAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private async Task ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	private void LoadData()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);
	}
}
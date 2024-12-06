using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uuc.Common.Collections;
using Uuc.Common.Extensions;
using Uuc.Models.Accounts;
using Uuc.PageModels.Base;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class AccountDetailsPageModel
(
	INavigationService navigationService,
	IAccountsService accountsService,
	IOperationsService operationsService
) : BasePageModel(navigationService)
{
	[ObservableProperty]
	private string _address = string.Empty;

	[ObservableProperty]
	private string _name = string.Empty;

	private readonly ObservableCollectionEx<Operation> _operations = new();
	public IReadOnlyList<Operation> Operations => _operations;

	public override async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetString("address", out string? address) && !string.IsNullOrEmpty(address))
		{
			await LoadData(address);
		}
	}

	private async Task LoadData(string address)
	{
		var account = await accountsService.FindAsync(address);
		Address = account.Address;
		Name = account.Name;

		var operations = await operationsService.ListByAccountAddressAsync(address);
		if (operations != null)
		{
			_operations.ReloadData(operations);
		}
	}

	[RelayCommand]
	private async Task OperationsList_OnTapped(Operation? operation)
	{
		if (operation != null)
		{
			Application.Current.MainPage.DisplayAlert(operation.SignerAddress, "signer", "ok");
		}
	}
}

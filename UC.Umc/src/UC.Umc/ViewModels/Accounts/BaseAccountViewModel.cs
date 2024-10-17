using System.Collections.ObjectModel;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Models.Common;

namespace UC.Umc.ViewModels.Accounts;

public abstract partial class BaseAccountViewModel(ILogger logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private ObservableCollection<AccountColor> _colorsCollection = new();
	
	[ObservableProperty]
	private ObservableCollection<DomainModel> _domains = new();
	
	[ObservableProperty]
	private ObservableCollection<ResourceModel> _resources = new();

	[ObservableProperty]
	private AccountModel _account;

	[ObservableProperty]
	private int _position;

	[RelayCommand]
	protected async Task CloseAsync() => await Navigation.PopModalAsync();

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
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
}

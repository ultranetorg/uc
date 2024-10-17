using System.Collections.ObjectModel;
using UC.Umc.Models;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Popups;

public partial class SelectDomainViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	public DomainModel SelectedAuthor;

	[ObservableProperty]
	private ObservableCollection<DomainModel> _domains = new();

	public SelectDomainViewModel(IServicesMockData service, ILogger<SelectDomainViewModel> logger) : base(logger)
	{
		_service = service;
		LoadData();
	}

	[RelayCommand]
	private void ItemTapped(DomainModel author)
	{
		foreach (var item in Domains)
		{
			item.IsSelected = false;
		}
		author.IsSelected = true;
		SelectedAuthor = author;
	}

	[RelayCommand]
	private void Close() => ClosePopup();
	
	public void LoadData()
	{
		var ownAuthors = _service.Domains.Where(x => x.Status == AuthorStatus.Owned).ToList();
		Domains.ReplaceAll(ownAuthors);
	}
}

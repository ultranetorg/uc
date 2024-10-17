using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class NotificationPopup : Popup
{
	private static NotificationPopup popup;

	public NotificationPopup()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<NotificationViewModel>();
	}

	public void Hide()
	{
		Close();
	}

	public static async Task Show()
	{
		popup = new NotificationPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}

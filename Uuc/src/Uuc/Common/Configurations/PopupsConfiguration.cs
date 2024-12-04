using CommunityToolkit.Maui;
using Uuc.PageModels.Popups;
using Uuc.Popups;

namespace Uuc.Common.Configurations;

public static class PopupsConfiguration
{
	public static MauiAppBuilder ConfigurePopups(this MauiAppBuilder builder)
	{
		builder.Services.AddTransientPopup<CreateAccountPopup, CreateAccountPopupModel>();
		builder.Services.AddTransientPopup<SendPopup, SendPopupModel>();
		builder.Services.AddTransientPopup<ReceivePopup, ReceivePopupModel>();

		return builder;
	}
}

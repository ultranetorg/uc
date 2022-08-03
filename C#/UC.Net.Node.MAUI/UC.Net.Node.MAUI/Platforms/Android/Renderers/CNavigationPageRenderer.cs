using Android.Content;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using UC.Net.Node.MAUI.Droid.Renderers;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CNavigationPageRenderer))]

namespace UC.Net.Node.MAUI.Droid.Renderers;

public class CNavigationPageRenderer : NavigationPageRenderer
{
	public CNavigationPageRenderer(Context context) : base(context)
	{
	}

	protected override Task<bool> OnPushAsync(Page view, bool animated)
	{
		return base.OnPushAsync(view, false);
	}

	protected override Task<bool> OnPopViewAsync(Page page, bool animated)
	{
		return base.OnPopViewAsync(page, false);
	}

	protected override Task<bool> OnPopToRootAsync(Page page, bool animated)
	{
		return base.OnPopToRootAsync(page, false);
	}
}
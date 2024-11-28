using Uuc.Pages;

namespace Uuc;

public partial class App : Application
{

	private readonly Shell _appShell;

	public App(AppShell appShell)
	{
		_appShell = appShell;

		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_appShell);
	}
}

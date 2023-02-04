using UC.Net;

namespace UC.Umc.ViewModels;

public partial class HelpViewModel : BaseViewModel
{
	[ObservableProperty]
    private Transaction _selectedItem ;

	[ObservableProperty]
    private CustomCollection<string> _helps = new();

    public HelpViewModel(ILogger<HelpViewModel> logger) : base(logger)
    {
		Initialize();
    }

	private void Initialize()
	{
		Helps.Clear();
        Helps.Add(Properties.Resources.HelpLine1);
        Helps.Add(Properties.Resources.HelpLine2);
        Helps.Add(Properties.Resources.HelpLine3);
        Helps.Add(Properties.Resources.HelpLine4);
        Helps.Add(Properties.Resources.HelpLine5);
        Helps.Add(Properties.Resources.HelpLine6);
        Helps.Add(Properties.Resources.HelpLine7);
        Helps.Add(Properties.Resources.HelpLine8);
        Helps.Add(Properties.Resources.HelpLine9);
        Helps.Add(Properties.Resources.HelpLine10);
        Helps.Add(Properties.Resources.HelpLine11);
        Helps.Add(Properties.Resources.HelpLine12);
        Helps.Add(Properties.Resources.HelpLine13);
	}
}

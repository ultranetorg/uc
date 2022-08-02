namespace UC.Net.Node.MAUI.Pages;

public partial class HelpPage : CustomPage
{
    public HelpPage()
    {
        InitializeComponent();
        BindingContext = new HelpViewModel(ServiceHelper.GetService<ILogger<HelpViewModel>>());
    }
}
public partial class HelpViewModel : BaseViewModel
{
       
    public HelpViewModel(ILogger<HelpViewModel> logger) : base(logger)
    {
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");

    }

	[RelayCommand]
    private async void Create()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async void Restore()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private void ItemTapped(string help)
    { 
    }

    Transaction _SelectedItem ;
    public Transaction SelectedItem
    {
        get { return _SelectedItem; }
        set { SetProperty(ref _SelectedItem, value); }
    }
    CustomCollection<string> _Helps = new CustomCollection<string>();
    public CustomCollection<string> Helps
    {
        get { return _Helps; }
        set { SetProperty(ref _Helps, value); }
    } 
}

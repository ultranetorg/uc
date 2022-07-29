namespace UC.Net.Node.MAUI.Popups;

public partial class SourceAccountPopup : Popup
{
    public SourceAccountPopup()
    {
        InitializeComponent();
        BindingContext = new SourceAccountViewModel(this, ServiceHelper.GetService<ILogger<SourceAccountViewModel>>());
    }

    public void Hide()
    {
		Close();
    }

    //Show returns popup.viewModel.Wallet (Task<Wallet>)
}

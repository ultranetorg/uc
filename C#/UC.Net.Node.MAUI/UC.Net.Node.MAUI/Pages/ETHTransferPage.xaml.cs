namespace UC.Net.Node.MAUI.Pages;

public partial class ETHTransferPage : CustomPage
{
    public ETHTransferPage()
    {
        InitializeComponent();
        BindingContext = new ETHTransferViewModel(ServiceHelper.GetService<ILogger<ETHTransferViewModel>>());
    }
}
public class ETHTransferViewModel : BaseViewModel
{
    public ETHTransferViewModel(ILogger<ETHTransferViewModel> logger) : base(logger)
    {
    }
       
    public Command CloseCommad
    {
        get => new Command(Close);
    }

    private async void Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
    public Command ConfirmCommand
    {
        get => new Command(Confirm);
    }
    private async void Confirm()
    {
        await Shell.Current.Navigation.PushAsync(new TransferCompletePage());
    }
    public Command PrevCommad
    {
        get => new Command(Prev);
    }
    private void Prev()
    {
        Position -= 1;
    }
    public Command NextCommad
    {
        get => new Command(Next);
    }
    private void Next()
    {
        if (Position == 2) return;
        Position += 1;
    }

    int _Position;
    public int Position
    {
        get { return _Position; }
        set { SetProperty(ref _Position, value); }
    }
}

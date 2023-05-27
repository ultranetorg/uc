namespace UC.Umc.Views;

public partial class Send2View : ContentView
{
    public static readonly BindableProperty SourceAccountProperty = BindableProperty.Create(nameof(SourceAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty RecipientAccountProperty = BindableProperty.Create(nameof(RecipientAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty AmountProperty = BindableProperty.Create(nameof(Amount), typeof(string), typeof(Send1View));

    public static readonly BindableProperty ComissionProperty = BindableProperty.Create(nameof(Comission), typeof(string), typeof(Send1View));

    public AccountViewModel SourceAccount
    {
        get { return (AccountViewModel)GetValue(SourceAccountProperty); }
        set { SetValue(SourceAccountProperty, value); }
    }

    public AccountViewModel RecipientAccount
    {
        get { return (AccountViewModel)GetValue(RecipientAccountProperty); }
        set { SetValue(RecipientAccountProperty, value); }
    }

    public string Amount
    {
        get { return (string)GetValue(AmountProperty); }
        set { SetValue(AmountProperty, value); }
    }

    public string Comission
    {
        get { return (string)GetValue(ComissionProperty); }
        set { SetValue(ComissionProperty, value); }
    }

    public Send2View()
    {
        InitializeComponent();
    }
}

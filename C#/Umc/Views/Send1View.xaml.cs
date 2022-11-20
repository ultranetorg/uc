using System.Windows.Input;

namespace UC.Umc.Views;

public partial class Send1View : ContentView
{
    public static readonly BindableProperty SourceAccountProperty =
		BindableProperty.Create(nameof(SourceAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty RecipientAccountProperty =
		BindableProperty.Create(nameof(RecipientAccount), typeof(AccountViewModel), typeof(Send1View));

    public static readonly BindableProperty SelectSourceAccountProperty =
		BindableProperty.Create(nameof(SelectSourceAccount), typeof(ICommand), typeof(Send1View));

    public static readonly BindableProperty SelectRecipientAccountProperty =
		BindableProperty.Create(nameof(SelectRecipientAccount), typeof(ICommand), typeof(Send1View));

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

    public ICommand SelectSourceAccount
    {
        get { return (ICommand)GetValue(SelectSourceAccountProperty); }
        set { SetValue(SelectSourceAccountProperty, value); }
    }

    public ICommand SelectRecipientAccount
    {
        get { return (ICommand)GetValue(SelectRecipientAccountProperty); }
        set { SetValue(SelectRecipientAccountProperty, value); }
    }

    public Send1View()
    {
        InitializeComponent();
    }
}

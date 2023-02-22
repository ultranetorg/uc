using System.Windows.Input;

namespace UC.Umc.Views;

public partial class ETHTransfer1View : ContentView
{
    public static readonly BindableProperty IsPrivateKeyProperty =
		BindableProperty.Create(nameof(IsPrivateKey), typeof(bool), typeof(AccountView));
    public static readonly BindableProperty IsFilePathProperty =
		BindableProperty.Create(nameof(IsFilePath), typeof(bool), typeof(AccountView));
    public static readonly BindableProperty ShowFilePasswordProperty =
		BindableProperty.Create(nameof(ShowFilePassword), typeof(bool), typeof(AccountView));
    public static readonly BindableProperty PrivateKeyProperty =
		BindableProperty.Create(nameof(PrivateKey), typeof(string), typeof(AccountView));
    public static readonly BindableProperty WalletFilePathProperty =
		BindableProperty.Create(nameof(WalletFilePath), typeof(string), typeof(AccountView));
    public static readonly BindableProperty WalletFilePasswordProperty =
		BindableProperty.Create(nameof(WalletFilePassword), typeof(string), typeof(AccountView));
    public static readonly BindableProperty EthAmountProperty =
		BindableProperty.Create(nameof(EthAmount), typeof(decimal), typeof(AccountView));
    public static readonly BindableProperty UntAmountProperty =
		BindableProperty.Create(nameof(UntAmount), typeof(decimal), typeof(AccountView));
    public static readonly BindableProperty ChangeKeySourceCommandProperty =
		BindableProperty.Create(nameof(ChangeKeySourceCommand), typeof(ICommand), typeof(AccountView));
    public static readonly BindableProperty OpenFilePickerCommandProperty =
		BindableProperty.Create(nameof(OpenFilePickerCommand), typeof(ICommand), typeof(AccountView));

	public bool IsPrivateKey
    {
        get { return (bool)GetValue(IsPrivateKeyProperty); }
        set { SetValue(IsPrivateKeyProperty, value); }
	}

	public bool IsFilePath
    {
        get { return (bool)GetValue(IsFilePathProperty); }
        set { SetValue(IsFilePathProperty, value); }
	}

	public bool ShowFilePassword
    {
        get { return (bool)GetValue(ShowFilePasswordProperty); }
        set { SetValue(ShowFilePasswordProperty, value); }
	}

	public string PrivateKey
    {
        get { return (string)GetValue(PrivateKeyProperty); }
        set { SetValue(PrivateKeyProperty, value); }
	}

	public string WalletFilePath
    {
        get { return (string)GetValue(WalletFilePathProperty); }
        set { SetValue(WalletFilePathProperty, value); }
	}

	public string WalletFilePassword
    {
        get { return (string)GetValue(WalletFilePasswordProperty); }
        set { SetValue(WalletFilePasswordProperty, value); }
	}

	public decimal EthAmount
    {
        get { return (decimal)GetValue(EthAmountProperty); }
        set { SetValue(EthAmountProperty, value); }
	}

	public decimal UntAmount
    {
        get { return (decimal)GetValue(UntAmountProperty); }
        set { SetValue(UntAmountProperty, value); }
	}

	public ICommand ChangeKeySourceCommand
    {
        get { return (ICommand)GetValue(ChangeKeySourceCommandProperty); }
        set { SetValue(ChangeKeySourceCommandProperty, value); }
	}

	public ICommand OpenFilePickerCommand
    {
        get { return (ICommand)GetValue(OpenFilePickerCommandProperty); }
        set { SetValue(OpenFilePickerCommandProperty, value); }
	}

    public ETHTransfer1View()
    {
        InitializeComponent();
    }
}
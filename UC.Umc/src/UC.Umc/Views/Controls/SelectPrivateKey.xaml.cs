using System.Windows.Input;

namespace UC.Umc.Views.Controls;

public partial class SelectPrivateKey : ContentView
{
	public static readonly BindableProperty SelectionChangedCommandProperty =
		BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(SelectPrivateKey));

	public static readonly BindableProperty IsPrivateKeyProperty =
		BindableProperty.Create(nameof(IsPrivateKey), typeof(bool), typeof(SelectPrivateKey));

	public static readonly BindableProperty IsWalletPathProperty =
		BindableProperty.Create(nameof(IsWalletPath), typeof(bool), typeof(SelectPrivateKey));

	public ICommand SelectionChangedCommand
	{
		get => (ICommand) GetValue(SelectionChangedCommandProperty);
		set => SetValue(SelectionChangedCommandProperty, value);
	}

	public bool IsPrivateKey
	{
		get => (bool) GetValue(IsPrivateKeyProperty);
		set => SetValue(IsPrivateKeyProperty, value);
	}

	public bool IsWalletPath
	{
		get => (bool) GetValue(IsWalletPathProperty);
		set => SetValue(IsWalletPathProperty, value);
	}

	public SelectPrivateKey()
	{
		InitializeComponent();
	}
}

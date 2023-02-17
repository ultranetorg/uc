using System.Windows.Input;

namespace UC.Umc.Controls;

public partial class SelectPrivateKey : ContentView
{
    public static readonly BindableProperty SelectionChangedProperty =
        BindableProperty.Create(nameof(SelectionChanged), typeof(ICommand), typeof(SelectPrivateKey), null,
            BindingMode.TwoWay);

	public static readonly BindableProperty IsPrivateKeyProperty =
        BindableProperty.Create(nameof(IsPrivateKey), typeof(bool), typeof(SelectPrivateKey), true);

    public ICommand SelectionChanged
    {
        get => (ICommand)GetValue(SelectionChangedProperty);
        set => SetValue(SelectionChangedProperty, value);
    }

    public bool IsPrivateKey
    {
        get => (bool)GetValue(IsPrivateKeyProperty);
        set => SetValue(IsPrivateKeyProperty, value);
    }

	public SelectPrivateKey()
	{
		InitializeComponent();
	}
}
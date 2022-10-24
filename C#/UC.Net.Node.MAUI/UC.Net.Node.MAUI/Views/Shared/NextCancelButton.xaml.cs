using System.Windows.Input;

namespace UC.Net.Node.MAUI.Controls;

public partial class NextCancelButton : StackLayout
{
	public static readonly BindableProperty NextCommadProperty = BindableProperty.Create(nameof(NextCommad), typeof(ICommand), typeof(NextCancelButton), null);

	public ICommand NextCommad
	{
		get { return (ICommand)GetValue(NextCommadProperty); }
		set { SetValue(NextCommadProperty, value); }
	}

	public NextCancelButton()
	{
		InitializeComponent();
	}
}
using System.Windows.Input;

namespace UC.Umc.Controls;

public partial class NextCancelButton : StackLayout
{
	public static readonly BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(NextCancelButton), null);

	public ICommand NextCommand
	{
		get { return (ICommand)GetValue(NextCommandProperty); }
		set { SetValue(NextCommandProperty, value); }
	}

	public NextCancelButton()
	{
		InitializeComponent();
	}
}
using System.Windows.Input;

namespace UC.Umc.Controls;

public partial class NextCancelButton : StackLayout
{
	public static readonly BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(NextCancelButton), null);

	public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(NextCancelButton), null);

	public ICommand NextCommand
	{
		get { return (ICommand)GetValue(NextCommandProperty); }
		set { SetValue(NextCommandProperty, value); }
	}

	public ICommand CancelCommand
	{
		get { return (ICommand)GetValue(CancelCommandProperty); }
		set { SetValue(CancelCommandProperty, value); }
	}

	public NextCancelButton()
	{
		InitializeComponent();
	}
}
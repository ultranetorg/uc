using System.Windows.Input;

namespace UC.Umc.Views.Controls;

public partial class NextCancelButton : StackLayout
{
	public static readonly BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(NextCancelButton), null);

	public ICommand NextCommand
	{
		get => (ICommand) GetValue(NextCommandProperty);
		set => SetValue(NextCommandProperty, value);
	}

	public NextCancelButton()
	{
		InitializeComponent();
	}
}

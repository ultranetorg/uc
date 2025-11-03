namespace UC.Umc.Controls;

public partial class ImageTextColumn : Grid
{
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ImageTextColumn), null);

	public static readonly BindableProperty IsEnabledImageProperty = BindableProperty.Create(nameof(IsEnabledImage), typeof(string), typeof(ImageTextColumn), null);

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	public string IsEnabledImage
	{
		get { return (string)GetValue(IsEnabledImageProperty); }
		set { SetValue(IsEnabledImageProperty, value); }
	}

	public ImageTextColumn()
	{
		InitializeComponent();
	}
}
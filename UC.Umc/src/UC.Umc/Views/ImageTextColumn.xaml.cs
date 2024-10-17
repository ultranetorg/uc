namespace UC.Umc.Views;

public partial class ImageTextColumn : Grid
{
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ImageTextColumn), null);

	public string Text
	{
		get => (string) GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public static readonly BindableProperty IsEnabledImageProperty = BindableProperty.Create(nameof(IsEnabledImage), typeof(string), typeof(ImageTextColumn), null);

	public string IsEnabledImage
	{
		get => (string) GetValue(IsEnabledImageProperty);
		set => SetValue(IsEnabledImageProperty, value);
	}

	public ImageTextColumn()
	{
		InitializeComponent();
	}
}

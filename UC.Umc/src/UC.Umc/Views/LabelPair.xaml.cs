namespace UC.Umc.Views;

public partial class LabelPair : Grid
{
	public static readonly BindableProperty Text1Property = BindableProperty.Create(nameof(Text1), typeof(string), typeof(LabelPair), null);
	public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(Text2), typeof(string), typeof(LabelPair), null);

	public string Text1
	{
		get => (string) GetValue(Text1Property);
		set => SetValue(Text1Property, value);
	}

	public string Text2
	{
		get => (string) GetValue(Text2Property);
		set => SetValue(Text2Property, value);
	}

	public LabelPair()
	{
		InitializeComponent();
	}
}

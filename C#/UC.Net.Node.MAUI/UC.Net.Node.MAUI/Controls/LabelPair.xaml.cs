namespace UC.Net.Node.MAUI.Controls;

public partial class LabelPair : Grid
{
	public static readonly BindableProperty Text1Property = BindableProperty.Create(nameof(Text1), typeof(string), typeof(LabelPair), null);

	public string Text1
	{
		get { return (string)GetValue(Text1Property); }
		set { SetValue(Text1Property, value); }
	}

	public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(Text2), typeof(string), typeof(LabelPair), null);

	public string Text2
	{
		get { return (string)GetValue(Text2Property); }
		set { SetValue(Text2Property, value); }
	}

	public LabelPair()
	{
		InitializeComponent();
	}
}
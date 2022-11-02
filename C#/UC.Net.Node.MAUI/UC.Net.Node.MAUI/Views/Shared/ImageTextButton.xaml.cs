using System.Windows.Input;
namespace UC.Net.Node.MAUI.Controls;

public partial class ImageTextButton : Grid
{
	#region Bindable Properties
	
	public static readonly BindableProperty ImageProperty
		= BindableProperty.Create(nameof(Image), typeof(ImageSource), typeof(ImageTextButton), null);

	public static readonly BindableProperty TextProperty
		= BindableProperty.Create(nameof(Text), typeof(string), typeof(ImageTextButton), null);

	public static readonly BindableProperty FontSizeProperty
		= BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ImageTextButton), 24.0);

	public static readonly BindableProperty ImageHeightProperty
		= BindableProperty.Create(nameof(ImageHeight), typeof(double), typeof(ImageTextButton), 24.0);

	public static readonly BindableProperty ImageWidthProperty
		= BindableProperty.Create(nameof(ImageWidth), typeof(double), typeof(ImageTextButton), 24.0);

	public static readonly BindableProperty TextColorProperty
		= BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ImageTextButton), Colors.Black);

	public static readonly BindableProperty ImageColorProperty
		= BindableProperty.Create(nameof(ImageColor), typeof(Color), typeof(ImageTextButton), null);

	public static readonly BindableProperty ImageSourceProperty
		= BindableProperty.Create(nameof(ImageSource), typeof(string), typeof(ImageTextButton), null);

	public static readonly BindableProperty ImageStyleProperty
		= BindableProperty.Create(nameof(ImageStyle), typeof(Style), typeof(ImageTextButton), null);

	public static readonly BindableProperty CommandProperty
		= BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageTextButton), null);

	public static readonly BindableProperty CommandParameterProperty
		= BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageTextButton), null);

	public Color ImageColor
	{
		get { return (Color)GetValue(ImageColorProperty); }
		set { SetValue(ImageColorProperty, value); }
	}

	public Color TextColor
	{
		get { return (Color)GetValue(TextColorProperty); }
		set { SetValue(TextColorProperty, value); }
	}

	public double ImageHeight
	{
		get { return (double)GetValue(ImageHeightProperty); }
		set { SetValue(TextProperty, value); }
	}

	public double ImageWidth
	{
		get { return (double)GetValue(ImageWidthProperty); }
		set { SetValue(TextProperty, value); }
	}

	public double FontSize
	{
		get { return (double)GetValue(FontSizeProperty); }
		set { SetValue(TextProperty, value); }
	}

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	public ImageSource Image
	{
		get { return (ImageSource)GetValue(ImageProperty); }
		set { SetValue(ImageProperty, value); }
	}

	public string ImageSource
	{
		get { return (string)GetValue(ImageSourceProperty); }
		set { SetValue(ImageSourceProperty, value); }
	}
	
	public Style ImageStyle
	{
		get { return (Style)GetValue(ImageStyleProperty); }
		set { SetValue(ImageStyleProperty, value); }
	}

	public ICommand Command
	{
		get { return (ICommand)GetValue(CommandProperty); }
		set { SetValue(CommandProperty, value); }
	}

	public object CommandParameter
	{
		get { return (object)GetValue(CommandParameterProperty); }
		set { SetValue(CommandParameterProperty, value); }
	}

	#endregion Bindable Properties

	public event EventHandler Clicked;

    public ImageTextButton()
    {
        InitializeComponent();
        Initialize();
    }

    public void Initialize()
    {
        var tap = new TapGestureRecognizer();
		// TODO: this will be remade, using TapGesture
        tap.Tapped += (sender, e) => Clicked?.Invoke(sender, e);
        tap.Command = Command;
		tap.CommandParameter = CommandParameter;
        GestureRecognizers.Add(tap);
    }
}
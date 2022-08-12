using System.Windows.Input;

namespace UC.Net.Node.MAUI.Controls;

public partial class MainPageButton : Frame
{
	#region Bindable Properties
	
	public static readonly BindableProperty ImageProperty = BindableProperty.Create("Image", typeof(ImageSource), typeof(MainPageButton), null);

	public ImageSource Image
	{
		get { return (ImageSource)GetValue(ImageProperty); }
		set { SetValue(ImageProperty, value); }
	}

	public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(MainPageButton), null);

	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	public static readonly BindableProperty DetailsProperty = BindableProperty.Create("Details", typeof(string), typeof(MainPageButton), null);

	public string Details
	{
		get { return (string)GetValue(DetailsProperty); }
		set { SetValue(DetailsProperty, value); }
	}

	public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(MainPageButton), null);

	public ICommand Command
	{
		get { return (ICommand)GetValue(CommandProperty); }
		set { SetValue(CommandProperty, value); }
	}

	public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(MainPageButton), null);

	public object CommandParameter
	{
		get { return GetValue(CommandParameterProperty); }
		set { SetValue(CommandParameterProperty, value); }
	}
	
	#endregion Bindable Properties

	public event EventHandler Clicked;

    public MainPageButton()
    {
        InitializeComponent();
        Initialize();
    }

    public void Initialize()
    {
        var tap = new TapGestureRecognizer();
		// TODO: this will be remade
        tap.Tapped += (sender, e) => Clicked?.Invoke(sender, e);
        tap.Command = Command;
		tap.CommandParameter = CommandParameter;
        GestureRecognizers.Add(tap);
    }
}
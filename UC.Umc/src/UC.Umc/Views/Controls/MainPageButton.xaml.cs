using System.Windows.Input;

namespace UC.Umc.Views.Controls;

public partial class MainPageButton : Frame
{
	#region Bindable Properties
	
	public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(Image), typeof(ImageSource), typeof(MainPageButton), null);

	public ImageSource Image
	{
		get => (ImageSource) GetValue(ImageProperty);
		set => SetValue(ImageProperty, value);
	}

	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MainPageButton), null);

	public string Text
	{
		get => (string) GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public static readonly BindableProperty DetailsProperty = BindableProperty.Create(nameof(Details), typeof(string), typeof(MainPageButton), null);

	public string Details
	{
		get => (string) GetValue(DetailsProperty);
		set => SetValue(DetailsProperty, value);
	}

	public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MainPageButton), null);

	public ICommand Command
	{
		get => (ICommand) GetValue(CommandProperty);
		set => SetValue(CommandProperty, value);
	}

	public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MainPageButton), null);

	public object CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
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

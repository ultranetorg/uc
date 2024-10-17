namespace UC.Umc.Views;

public partial class UpdatesList : ContentView
{
	public static readonly BindableProperty AddedListProperty =
		BindableProperty.Create(nameof(AddedList), typeof(List<string>), typeof(UpdatesList));
	public static readonly BindableProperty FixedListProperty =
		BindableProperty.Create(nameof(FixedList), typeof(List<string>), typeof(UpdatesList));

	public List<string> AddedList
	{
		get => (List<string>) GetValue(AddedListProperty);
		set => SetValue(AddedListProperty, value);
	}
	public List<string> FixedList
	{
		get => (List<string>) GetValue(FixedListProperty);
		set => SetValue(FixedListProperty, value);
	}

	public UpdatesList()
	{
		InitializeComponent();
	}
}

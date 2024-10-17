namespace UC.Umc.Views;

public partial class CustomPage : ContentPage
{
	public static readonly BindableProperty MainContentProperty = BindableProperty.Create(nameof(MainContent), typeof(View), typeof(CustomPage), null);

	public static readonly BindableProperty AnimationEnabledProperty = BindableProperty.Create(nameof(AnimationEnabled), typeof(bool), typeof(CustomPage), false);

	public View MainContent
	{
		get => (View) GetValue(MainContentProperty);
		set => SetValue(MainContentProperty, value);
	}

	public bool AnimationEnabled
	{
		get => (bool) GetValue(AnimationEnabledProperty);
		set => SetValue(AnimationEnabledProperty, value);
	}

	public CustomPage()
	{
		InitializeComponent();
	}

	//protected override async void OnAppearing()
	//{
	//	base.OnAppearing();
	//	if (AnimationEnabled)
	//	{
	//		MainFrame.TranslationY = Height;
	//		await MainFrame.TranslateTo(0, 0, LENGTH, Easing.Linear);
	//	}
	//}

	//protected override async void OnDisappearing()
	//{
	//	base.OnDisappearing();
	//	if (AnimationEnabled)
	//		await MainFrame.TranslateTo(0, Height, LENGTH, Easing.Linear);
	//}
}

namespace UC.Umc.Controls;

public partial class CustomPage : ContentPage
{
	public static readonly BindableProperty MainContentProperty = BindableProperty.Create(nameof(MainContent), typeof(View), typeof(CustomPage), null);

	public static readonly BindableProperty EnableBackButtonProperty = BindableProperty.Create(nameof(EnableBackButton), typeof(bool), typeof(CustomPage), true);

	public View MainContent
	{
		get { return (View)GetValue(MainContentProperty); }
		set { SetValue(MainContentProperty, value); }
	}

	public bool EnableBackButton
	{
		get { return (bool)GetValue(EnableBackButtonProperty); }
		set { SetValue(EnableBackButtonProperty, value); }
	}

	public CustomPage()
    {
        InitializeComponent();
    }

	#region Animation: TBD

	// private const int LENGTH = 220;

	// public static readonly BindableProperty AnimationEnabledProperty = BindableProperty.Create(nameof(AnimationEnabled), typeof(bool), typeof(CustomPage), false);

	//public bool AnimationEnabled
	//{
	//	get { return (bool)GetValue(AnimationEnabledProperty); }
	//	set { SetValue(AnimationEnabledProperty, value); }
	//}

	//protected override async void OnAppearing()
	//{
	//    base.OnAppearing();
	//    if (AnimationEnabled)
	//    {
	//        MainFrame.TranslationY = Height;
	//        await MainFrame.TranslateTo(0, 0, LENGTH, Easing.Linear);
	//    }
	//}

	//protected override async void OnDisappearing()
	//{
	//    base.OnDisappearing();
	//    if (AnimationEnabled)
	//        await MainFrame.TranslateTo(0, Height, LENGTH, Easing.Linear);
	//}

	#endregion Animation: TBD
}
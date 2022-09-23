namespace UC.Net.Node.MAUI.Controls;

public partial class CustomPage : ContentPage
{
	private const int LENGTH = 220;

	public static readonly BindableProperty RightContentProperty = BindableProperty.Create(nameof(RightContent), typeof(View), typeof(CustomPage), null);

	public static readonly BindableProperty HasBackButtonProperty = BindableProperty.Create(nameof(HasBackButton), typeof(bool), typeof(CustomPage), true);

	public static readonly BindableProperty AnimationEnabledProperty = BindableProperty.Create(nameof(AnimationEnabled), typeof(bool), typeof(CustomPage), false);

	public static readonly BindableProperty MainContentProperty = BindableProperty.Create(nameof(MainContent), typeof(View), typeof(CustomPage), null);

	public View MainContent
	{
		get { return (View)GetValue(MainContentProperty); }
		set { SetValue(MainContentProperty, value); }
	}

	public bool AnimationEnabled
	{
		get { return (bool)GetValue(AnimationEnabledProperty); }
		set { SetValue(AnimationEnabledProperty, value); }
	}

	public bool HasBackButton
	{
		get { return (bool)GetValue(HasBackButtonProperty); }
		set { SetValue(HasBackButtonProperty, value); }
	}

	public View RightContent
	{
		get { return (View)GetValue(RightContentProperty); }
		set { SetValue(RightContentProperty, value); }
	}

	public CustomPage()
    {
        InitializeComponent();
    }

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

    //private async void BackButtonClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}
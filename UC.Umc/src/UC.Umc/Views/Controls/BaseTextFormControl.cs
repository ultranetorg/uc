#if IOS
using UIKit;
#elif ANDROID
using Android.Content.Res;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif

using System.Windows.Input;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Handlers;

namespace UC.Umc.Views.Controls;

public abstract class BaseTextFormControl<T> : BaseFormControl<T, string>
{
	private const int DEFAULT_MIN_LENGTH = 0;

	public static readonly BindableProperty IsReadOnlyProperty =
		BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(T), false);

	public static readonly BindableProperty IsTextPredictionEnabledProperty =
		BindableProperty.Create(nameof(IsTextPredictionEnabled), typeof(bool), typeof(T), true,
			BindingMode.OneTime);

	public static readonly BindableProperty KeyboardProperty =
		BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(T),
			Keyboard.Default, coerceValue: (_, v) => (Keyboard)v ?? Keyboard.Default);

	public static readonly BindableProperty MinLengthProperty =
		BindableProperty.Create(nameof(MinLength), typeof(int), typeof(T), DEFAULT_MIN_LENGTH);

	public static readonly BindableProperty RegexValidationPatternProperty =
	   BindableProperty.Create(nameof(RegexValidationPattern), typeof(string), typeof(T));

	public static readonly BindableProperty TextChangedProperty =
		BindableProperty.Create(nameof(TextChanged), typeof(ICommand), typeof(T), null,
			BindingMode.TwoWay);

	public bool IsReadOnly
	{
		get => (bool) GetValue(IsReadOnlyProperty);
		set => SetValue(IsReadOnlyProperty, value);
	}

	public bool IsTextPredictionEnabled
	{
		get => (bool) GetValue(IsTextPredictionEnabledProperty);
		set => SetValue(IsTextPredictionEnabledProperty, value);
	}

	[System.ComponentModel.TypeConverter(typeof(KeyboardTypeConverter))]
	public Keyboard Keyboard
	{
		get => (Keyboard) GetValue(KeyboardProperty);
		set => SetValue(KeyboardProperty, value);
	}

	public int MinLength
	{
		get => (int) GetValue(MinLengthProperty);
		set => SetValue(MinLengthProperty, value);
	}

	public string RegexValidationPattern
	{
		get => (string) GetValue(RegexValidationPatternProperty);
		set => SetValue(RegexValidationPatternProperty, value);
	}

	public ICommand TextChanged
	{
		get => (ICommand) GetValue(TextChangedProperty);
		set => SetValue(TextChangedProperty, value);
	}

	/// <inheritdoc/>
	protected override void ControlHandlerMapper<Z>(Z handler, IView _)
	{
#if IOS
        const UITextBorderStyle borderStyler = UITextBorderStyle.None;
        var backgroundColor = UIColor.Clear;
#elif ANDROID
		var backgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif

		switch (handler)
		{
			case IEntryHandler entryHandler:
#if IOS
                entryHandler.PlatformView.BorderStyle = borderStyler;
#elif ANDROID
				entryHandler.PlatformView.BackgroundTintList = backgroundTintList;
#endif
				break;
		}
	}
}

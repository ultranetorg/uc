#if IOS
using UIKit;
#elif ANDROID
using Android.Content.Res;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif
using Microsoft.Maui.Handlers;

namespace UC.Umc.Controls;

public abstract class BaseFormControl<T, U> : ContentView
{
    protected readonly ILogger<T> Logger;

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(T));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(T));

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(U), typeof(T), null, BindingMode.TwoWay);


    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set
        {
            SetControlStyles();
            SetValue(ErrorMessageProperty, value);
        }
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public U Value
    {
        get
        {
            SetControlStyles();
            return (U)GetValue(ValueProperty);
        }
        set
        {
            SetControlStyles();
            SetValue(ValueProperty, value);
        }
    }

    protected virtual string FormControlType => string.Empty;

    protected abstract void ControlHandler();

    protected BaseFormControl()
    {
        Logger = Ioc.Default.GetService<ILogger<T>>();
    }

    protected virtual void SetControlStyles()
    {
        try
        {
            const string activeBorderStyle = "ActiveBorderStyle";
            const string activeSpanStyle = "ActiveSpanStyle";
            var activeControlStyle = $"Active{FormControlType}Style";

            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                Resources[activeBorderStyle] = Resources["ValidBorderStyle"];
                Resources[activeSpanStyle] = Resources["ValidTextStyle"];

                if (!string.IsNullOrWhiteSpace(FormControlType))
                {
                    Resources[activeControlStyle] = Resources[$"Valid{FormControlType}Style"];
                }
            }
            else
            {
                Resources[activeBorderStyle] = Resources["InvalidBorderStyle"];
                Resources[activeSpanStyle] = Resources["InvalidTextStyle"];

                if (!string.IsNullOrWhiteSpace(FormControlType))
                {
                    Resources[activeControlStyle] = Resources[$"Invalid{FormControlType}Style"];
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SetControlStyles Error: {Ex}", ex.Message);
        }
    }

    /// <summary>
    /// Removes Underline from Android Inputs and Border from iOS Inputs
    /// </summary>
    protected virtual void ControlHandlerMapper<Z>(Z handler, IView _) where Z : IViewHandler
    {
#if IOS
        const UITextBorderStyle borderStyler = UITextBorderStyle.None;
#elif ANDROID
        var backgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif

        switch (handler)
        {
            case IDatePickerHandler datePickerHandler:
#if IOS
                datePickerHandler.PlatformView.BorderStyle = borderStyler;
#elif ANDROID
                datePickerHandler.PlatformView.BackgroundTintList = backgroundTintList;
#endif
                break;
            case ITimePickerHandler timePickerHandler:
#if IOS
                timePickerHandler.PlatformView.BorderStyle = borderStyler;
#elif ANDROID
                timePickerHandler.PlatformView.BackgroundTintList = backgroundTintList;
#endif
                break;
        }
    }
}

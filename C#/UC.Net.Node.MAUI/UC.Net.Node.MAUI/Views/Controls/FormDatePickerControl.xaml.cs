using Microsoft.Maui.Handlers;

namespace UC.Net.Node.MAUI.Controls;

public sealed partial class FormDatePickerControl : BaseFormControl<FormDatePickerControl, DateTime>
{
    public static readonly BindableProperty FormatProperty =
        BindableProperty.Create(nameof(Format), typeof(string), typeof(FormTimePickerControl), "D");

    public static readonly BindableProperty MaxDateProperty =
        BindableProperty.Create(nameof(MaxDate), typeof(DateTime), typeof(FormDatePickerControl),
        DateTime.Now.AddYears(5));

    public static readonly BindableProperty MinDateProperty =
        BindableProperty.Create(nameof(MinDate), typeof(DateTime), typeof(FormDatePickerControl), DateTime.Now);

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public DateTime MaxDate
    {
        get => (DateTime)GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
    }

    public DateTime MinDate
    {
        get => (DateTime)GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    public FormDatePickerControl()
    {
        InitializeComponent();
        ControlHandler();
    }

    protected override void ControlHandler()
    {
        try
        {
            DatePickerHandler.Mapper.AppendToMapping("FormDateControlMods", ControlHandlerMapper);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "DateHandler Error: {Ex}", ex.Message);
        }
    }
}


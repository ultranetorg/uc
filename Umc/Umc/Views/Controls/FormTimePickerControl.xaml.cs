using Microsoft.Maui.Handlers;

namespace UC.Umc.Controls;

public sealed partial class FormTimePickerControl : BaseFormControl<FormTimePickerControl, TimeSpan>
{
    public static readonly BindableProperty FormatProperty = BindableProperty.Create(nameof(Format), typeof(string), typeof(FormTimePickerControl), "t");

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public FormTimePickerControl()
    {
        InitializeComponent();
        ControlHandler();
    }

    protected override void ControlHandler()
    {
        try
        {
            TimePickerHandler.Mapper.AppendToMapping("FormTimeControlMods", ControlHandlerMapper);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "TimePickerHandler Error: {Ex}", ex.Message);
        }
    }
}

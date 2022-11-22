using Microsoft.Maui.Handlers;

namespace UC.Umc.Controls;

public sealed partial class FormNumericControl : BaseTextFormControl<FormNumericControl>
{
    private const int DEFAULT_MAX_VALUE = int.MaxValue;

    public static readonly BindableProperty ClearButtonVisibilityProperty =
        BindableProperty.Create(nameof(ClearButtonVisibility), typeof(ClearButtonVisibility), typeof(FormNumericControl),
            ClearButtonVisibility.Never);

    public static readonly BindableProperty MaxValueProperty =
        BindableProperty.Create(nameof(MaxValue), typeof(int), typeof(FormNumericControl), DEFAULT_MAX_VALUE);

    public static readonly BindableProperty IsNotValidProperty =
        BindableProperty.Create(nameof(IsNotValid), typeof(bool), typeof(FormNumericControl));

    public ClearButtonVisibility ClearButtonVisibility
    {
        get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
        set => SetValue(ClearButtonVisibilityProperty, value);
    }

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public bool IsNotValid
    {
        get => (bool)GetValue(IsNotValidProperty);
        set => SetValue(IsNotValidProperty, value);
    }

    protected override string FormControlType => nameof(Entry);

    public FormNumericControl()
    {
        InitializeComponent();
        ControlHandler();
    }

    protected override void ControlHandler()
    {
        try
        {
            EntryHandler.Mapper.AppendToMapping("FormEntryControlMods", ControlHandlerMapper);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "EntryHandler Error: {Ex}", ex.Message);
        }
    }
}

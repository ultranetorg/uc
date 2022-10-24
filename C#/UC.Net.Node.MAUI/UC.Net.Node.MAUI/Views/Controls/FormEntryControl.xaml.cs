using Microsoft.Maui.Handlers;

namespace UC.Net.Node.MAUI.Controls;

public sealed partial class FormEntryControl : BaseTextFormControl<FormEntryControl>
{
    private const int DEFAULT_MAX_LENGTH = 100;

    public static readonly BindableProperty ClearButtonVisibilityProperty =
        BindableProperty.Create(nameof(ClearButtonVisibility), typeof(ClearButtonVisibility), typeof(FormEntryControl),
            ClearButtonVisibility.Never);

    public static readonly BindableProperty EntryMaskProperty =
        BindableProperty.Create(nameof(EntryMask), typeof(string), typeof(FormEntryControl));

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(FormEntryControl), DEFAULT_MAX_LENGTH);

    public ClearButtonVisibility ClearButtonVisibility
    {
        get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
        set => SetValue(ClearButtonVisibilityProperty, value);
    }

    public string EntryMask
    {
        get => (string)GetValue(EntryMaskProperty);
        set => SetValue(EntryMaskProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    protected override string FormControlType => nameof(Entry);

    public FormEntryControl()
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

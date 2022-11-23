using Microsoft.Maui.Handlers;
using System.Windows.Input;

namespace UC.Umc.Controls;

public sealed partial class FormNumericControl : BaseFormControl<FormNumericControl, decimal>
{
    private const int DEFAULT_MAX_VALUE = int.MaxValue;

    public static readonly BindableProperty ClearButtonVisibilityProperty =
        BindableProperty.Create(nameof(ClearButtonVisibility), typeof(ClearButtonVisibility), typeof(FormNumericControl),
            ClearButtonVisibility.Never);

    public static readonly BindableProperty MaxValueProperty =
        BindableProperty.Create(nameof(MaxValue), typeof(int), typeof(FormNumericControl), DEFAULT_MAX_VALUE);

    public static readonly BindableProperty IsNotValidProperty =
        BindableProperty.Create(nameof(IsNotValid), typeof(bool), typeof(FormNumericControl));

    public static readonly BindableProperty TextChangedProperty =
        BindableProperty.Create(nameof(TextChanged), typeof(ICommand), typeof(FormNumericControl), null,
            BindingMode.TwoWay);

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

    public ICommand TextChanged
    {
        get => (ICommand)GetValue(TextChangedProperty);
        set => SetValue(TextChangedProperty, value);
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

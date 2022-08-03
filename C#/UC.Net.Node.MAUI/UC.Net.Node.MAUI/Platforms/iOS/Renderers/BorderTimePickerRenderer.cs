using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.iOS.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Platform;
using UIKit;

[assembly: ExportRenderer(typeof(BorderTimePicker), typeof(BorderTimePickerRenderer))]
namespace UC.Net.Node.MAUI.iOS.Renderers;

public class BorderTimePickerRenderer : TimePickerRenderer
{
    private BorderTimePicker element;

    protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
    {
        base.OnElementChanged(e);

        if (e.OldElement != null || e.NewElement == null)
            return;

        element = (BorderTimePicker)this.Element;
        Control.BorderStyle = UITextBorderStyle.None;
        UpdateBorderWidth();
        UpdateBorderColor();
        UpdateBorderRadius();
        Control.ClipsToBounds = true;
    }

    protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnElementPropertyChanged(sender, e);
        if (this.Element == null)
            return;
        if (e.PropertyName == BorderTimePicker.BorderWidthProperty.PropertyName)
        {
            UpdateBorderWidth();
        }
        else if (e.PropertyName == BorderTimePicker.BorderColorProperty.PropertyName)
        {
            UpdateBorderColor();
        }
        else if (e.PropertyName == BorderTimePicker.BorderRadiusProperty.PropertyName)
        {
            UpdateBorderRadius();
        }
    }

    private void UpdateBorderWidth()
    {
        var TimePickerEx = this.Element as BorderTimePicker;
        Control.Layer.BorderWidth = TimePickerEx.BorderWidth;
    }

    private void UpdateBorderColor()
    {
        var TimePickerEx = this.Element as BorderTimePicker;
        Control.Layer.BorderColor = TimePickerEx.BorderColor.ToPlatform().CGColor;
    }

    private void UpdateBorderRadius()
    {
        var TimePickerEx = this.Element as BorderTimePicker;
        Control.Layer.CornerRadius = (float)TimePickerEx.BorderRadius;
    }
}
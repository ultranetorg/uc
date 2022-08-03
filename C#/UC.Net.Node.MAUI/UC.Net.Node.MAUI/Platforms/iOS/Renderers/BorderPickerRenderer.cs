using UIKit;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.iOS.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Platform;

[assembly: ExportRenderer(typeof(BorderPicker), typeof(BorderPickerRenderer))]
namespace UC.Net.Node.MAUI.iOS.Renderers;

public class BorderPickerRenderer : PickerRenderer
{
    private BorderPicker element;

    protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
    {
        base.OnElementChanged(e);

        if (e.OldElement != null || e.NewElement == null)
            return;

        element = (BorderPicker)this.Element;
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
        if (e.PropertyName == BorderPicker.BorderWidthProperty.PropertyName)
        {
            UpdateBorderWidth();
        }
        else if (e.PropertyName == BorderPicker.BorderColorProperty.PropertyName)
        {
            UpdateBorderColor();
        }
        else if (e.PropertyName == BorderPicker.BorderRadiusProperty.PropertyName)
        {
            UpdateBorderRadius();
        }
           
    }

    private void UpdateBorderWidth()
    {
        var PickerEx = this.Element as BorderPicker;
        Control.Layer.BorderWidth = PickerEx.BorderWidth;
    }

    private void UpdateBorderColor()
    {
        var PickerEx = this.Element as BorderPicker;
        Control.Layer.BorderColor = PickerEx.BorderColor.ToPlatform().CGColor;
    }

    private void UpdateBorderRadius()
    {
        var PickerEx = this.Element as BorderPicker;
        Control.Layer.CornerRadius = (float)PickerEx.BorderRadius;
    }
}
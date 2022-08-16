using UIKit;
using UC.Net.Node.MAUI.iOS;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRender))]
namespace UC.Net.Node.MAUI.iOS;

public class CustomDatePickerRender : DatePickerRenderer
{
    protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
    {
        base.OnElementChanged(e);
        DatePicker view = e.NewElement;
        Control.BorderStyle = UITextBorderStyle.None;
    }
}

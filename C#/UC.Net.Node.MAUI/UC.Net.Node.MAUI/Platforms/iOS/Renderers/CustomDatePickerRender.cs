using UC.Net.Node.MAUI.iOS;
using UIKit;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRender))]
namespace UC.Net.Node.MAUI.iOS;

class CustomDatePickerRender : DatePickerRenderer
{
    protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
    {
        base.OnElementChanged(e);
        var view = e.NewElement as DatePicker;
        this.Control.BorderStyle = UITextBorderStyle.None;
    }
}

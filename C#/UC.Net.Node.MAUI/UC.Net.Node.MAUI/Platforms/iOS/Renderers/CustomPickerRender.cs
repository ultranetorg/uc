using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using UIKit;
using UC.Net.Node.MAUI.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRender))]
namespace UC.Net.Node.MAUI.iOS;

class CustomPickerRender : PickerRenderer
{
    protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
    {
        base.OnElementChanged(e);
        var view = e.NewElement as Picker;
        this.Control.BorderStyle = UITextBorderStyle.None;
    }
}

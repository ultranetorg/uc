using Android.Content;
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.Droid;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace UC.Net.Node.MAUI.Droid;

class CustomPickerRenderer : PickerRenderer
{
    public CustomPickerRenderer(Context context) : base(context)
    {
    }

    protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
    {
        base.OnElementChanged(e);
        if (Control != null)
        {
            GradientDrawable gd = new();
            gd.SetStroke(0, Android.Graphics.Color.Transparent);
            Control.SetBackground(gd);
        }
    }
}
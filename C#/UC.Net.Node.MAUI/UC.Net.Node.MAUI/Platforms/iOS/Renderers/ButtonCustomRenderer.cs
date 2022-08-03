using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.iOS.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(Button), typeof(ButtonCustomRenderer))]
namespace UC.Net.Node.MAUI.iOS.Renderers;

// MaterialButtonRenderer has been replaced 
public class ButtonCustomRenderer : ButtonRenderer
{
    protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
    {
        base.OnElementChanged(e);
        if (Element != null)
        {
            Element.TextTransform = TextTransform.None;
        }
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();

        // Left-align the image and center the text
        // Taken from https://stackoverflow.com/a/71044012/238419
        if (Element.ContentLayout.Position == Button.ButtonContentLayout.ImagePosition.Right)
        {
            const int imageMargin = 10; // This might need to get multiplied by the screen density, not sure yet.  I'll update this later if it does.
            float imageOffset = (float)(Control.ImageView.Frame.Right + imageMargin);
		// TODO: unsupported on ios 15.0 and later
            Control.ImageEdgeInsets = new UIEdgeInsets(0, imageOffset, 0, imageOffset);

            float textOffset = (float)(Control.TitleLabel.Frame.Right + (Control.Frame.Width - Control.TitleLabel.Frame.Width) / 2);
            Control.TitleEdgeInsets = new UIEdgeInsets(0, textOffset, 0,textOffset);
        }
    }
}
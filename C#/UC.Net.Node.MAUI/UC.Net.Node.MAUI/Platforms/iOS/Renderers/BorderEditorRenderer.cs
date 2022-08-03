using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.iOS.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Platform;

[assembly: ExportRenderer(typeof(BorderEditor), typeof(BorderEditorRenderer))]
namespace UC.Net.Node.MAUI.iOS.Renderers;

// TODO replace obsolete
public class BorderEditorRenderer : EditorRenderer
{
    private BorderEditor element;
    private BorderEditor EditorEx;

    protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
    {
        base.OnElementChanged(e);

        if (e.OldElement != null || e.NewElement == null)
            return;

        element = (BorderEditor)this.Element;
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
        if (e.PropertyName == BorderEditor.BorderWidthProperty.PropertyName)
        {
            UpdateBorderWidth();
        }
        else if (e.PropertyName == BorderEditor.BorderColorProperty.PropertyName)
        {
            UpdateBorderColor();
        }
        else if (e.PropertyName == BorderEditor.BorderRadiusProperty.PropertyName)
        {
            UpdateBorderRadius();
        }
        else if (e.PropertyName == BorderEditor.LineColorProperty.PropertyName)
        {
            UpdateLineColor();
        }
    }

    private void UpdateLineColor()
    {
        if (Control != null)
        {
            EditorEx = this.Element as BorderEditor;
            CALayer border = new CALayer();
            float width = 2.0f;
            border.BorderColor = EditorEx.LineColor.ToCGColor();
            border.Frame = new CGRect(0, 40, 400, 2.0f);
            border.BorderWidth = width;

            Control.Layer.AddSublayer(border);
            Control.Layer.MasksToBounds = true;
        }
    }

    private void UpdateBorderWidth()
    {
        EditorEx = this.Element as BorderEditor;
        Control.Layer.BorderWidth = EditorEx.BorderWidth;
    }

    private void UpdateBorderColor()
    {
        EditorEx = this.Element as BorderEditor;
        Control.Layer.BorderColor = EditorEx.BorderColor.ToPlatform().CGColor;
    }

    private void UpdateBorderRadius()
    {
            EditorEx = this.Element as BorderEditor;
        Control.Layer.CornerRadius = (float)EditorEx.BorderRadius;
    }
}
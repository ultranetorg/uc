using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using UC.Net.Node.MAUI.iOS.Renderers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Platform;
using UIKit;

[assembly: ExportRenderer(typeof(BorderEntry), typeof(BorderEntryRenderer))]
namespace UC.Net.Node.MAUI.iOS.Renderers;

public class BorderEntryRenderer : EntryRenderer
{
    private BorderEntry element;
    private BorderEntry entryEx;

    protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
    {
        base.OnElementChanged(e);

        if (e.OldElement != null || e.NewElement == null)
            return;

        element = (BorderEntry)this.Element;
        Control.BorderStyle = UITextBorderStyle.None;
        UpdateBorderWidth();
        UpdateBorderColor();
        UpdateBorderRadius();
        UpdateRightPadding();
        UpdateLeftPadding();
        UpdateAutoFocus();
        Control.ClipsToBounds = true;
    }

    protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnElementPropertyChanged(sender, e);
        if (this.Element == null)
            return;
        if (e.PropertyName == BorderEntry.BorderWidthProperty.PropertyName)
        {
            UpdateBorderWidth();
        }
        else if (e.PropertyName == BorderEntry.BorderColorProperty.PropertyName)
        {
            UpdateBorderColor();
        }
        else if (e.PropertyName == BorderEntry.BorderRadiusProperty.PropertyName)
        {
            UpdateBorderRadius();
        }
        else if (e.PropertyName == BorderEntry.LineColorProperty.PropertyName)
        {
            UpdateLineColor();
        }
        else if (e.PropertyName == BorderEntry.LeftPaddingProperty.PropertyName)
        {
            UpdateLeftPadding();
        }
        else if (e.PropertyName == BorderEntry.RightPaddingProperty.PropertyName)
        {
            UpdateRightPadding();
        }
        else if (e.PropertyName == BorderEntry.AutoFocusProperty.PropertyName)
        {
            UpdateAutoFocus();
        }
    }

    public void UpdateAutoFocus()
    {
        entryEx = this.Element as BorderEntry;
        if (!entryEx.AutoFocus)
        {
            this.Control.InputView = new UIView();
        }
    }

    private void UpdateLineColor()
    {
        if (Control != null)
        {
            Control.BorderStyle = UITextBorderStyle.None;

            entryEx = this.Element as BorderEntry;
            CALayer border = new CALayer();
            float width = 2.0f;
            border.BorderColor = entryEx.LineColor.ToCGColor();
            border.Frame = new CGRect(0, 40, 400, 2.0f);
            border.BorderWidth = width;

            Control.Layer.AddSublayer(border);
            Control.Layer.MasksToBounds = true;
        }
    }

    private void UpdateBorderWidth()
    {
        entryEx = this.Element as BorderEntry;
        Control.Layer.BorderWidth = entryEx.BorderWidth;
    }

    private void UpdateBorderColor()
    {
        entryEx = this.Element as BorderEntry;
        Control.Layer.BorderColor = entryEx.BorderColor.ToPlatform().CGColor;
    }

    private void UpdateBorderRadius()
    {
        entryEx = this.Element as BorderEntry;
        Control.Layer.CornerRadius = (float)entryEx.BorderRadius;
    }

    private void UpdateLeftPadding()
    {
        entryEx = this.Element as BorderEntry;
        Control.LeftView = new UIView(new CGRect(0,0, entryEx.LeftPadding, 0));
        Control.LeftViewMode = UITextFieldViewMode.Always;
    }

    private void UpdateRightPadding()
    {
        entryEx = this.Element as BorderEntry;
        Control.RightView = new UIView(new CGRect(0, 0, entryEx.RightPadding, 0));
        Control.RightViewMode = UITextFieldViewMode.Always;
    }
}
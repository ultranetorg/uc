using Android.Graphics.Drawables;
using Color = Microsoft.Maui.Graphics.Color;

namespace UC.Net.Node.MAUI.Droid.Randerers;

public class BorderRenderer : IDisposable
{
    private GradientDrawable _background;

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }
        }
    }

    public Drawable GetBorderBackground(Color borderColor, Color backgroundColor, float borderWidth, float borderRadius)
    {
        if (_background != null)
        {
            _background.Dispose();
            _background = null;
        }
		// TODO
        //borderWidth = borderWidth > 0 ? borderWidth : 0;
        //borderRadius = borderRadius > 0 ? borderRadius : 0;
        //borderColor = borderColor != new Color() ? borderColor : Colors.Transparent;
        //backgroundColor = backgroundColor != new Colors() ? backgroundColor : Colors.Transparent;

        //var strokeWidth = Xamarin.Forms.Forms.Context.ToPixels(borderWidth);
        //var radius = Xamarin.Forms.Forms.Context.ToPixels(borderRadius);
        //_background = new GradientDrawable();
        //_background.SetColor(backgroundColor.ToAndroid());
        //if (radius > 0)
        //    _background.SetCornerRadius(radius);
        //if (borderColor != Colors.Transparent && strokeWidth > 0)
        //{
        //    _background.SetStroke((int)strokeWidth, borderColor.ToAndroid());
        //}
        return _background;
    }
}
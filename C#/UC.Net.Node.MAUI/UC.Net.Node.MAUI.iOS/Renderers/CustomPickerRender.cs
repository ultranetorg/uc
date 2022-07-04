using UC.Net.Node.MAUI.iOS;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRender))]
namespace UC.Net.Node.MAUI.iOS
{
    class CustomPickerRender : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            var view = e.NewElement as Picker;
            this.Control.BorderStyle = UITextBorderStyle.None;
        }
    }
}

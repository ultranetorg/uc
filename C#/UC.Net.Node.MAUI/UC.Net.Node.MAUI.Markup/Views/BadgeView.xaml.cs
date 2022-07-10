using UC.Net.Node.MAUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UC.Net.Node.MAUI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BadgeView : ContentView
    {
        public static readonly BindableProperty BadgeValueProperty = BindableProperty.Create("BadgeValue", typeof(string), typeof(BadgeView), string.Empty);
        public string BadgeValue
        {
            get { return (string)GetValue(BadgeValueProperty); }
            set { SetValue(BadgeValueProperty, value); }
        }
        public BadgeView()
        {
            InitializeComponent();
        }
    }
}
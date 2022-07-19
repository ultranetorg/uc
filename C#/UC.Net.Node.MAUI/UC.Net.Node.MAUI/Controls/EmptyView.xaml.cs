namespace UC.Net.Node.MAUI.Controls
{
    public partial class EmptyView : ContentView
    {
        public EmptyView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(EmptyView), null);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
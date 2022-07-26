using CommunityToolkit.Maui.Views;

namespace UC.Net.Node.MAUI.Popups
{
    public partial class AlertPopup : Popup
    {
        public AlertPopup(string message)
        {
            InitializeComponent();

            Message = message;
			Size = PopupSizeConstants.AutoCompleteControl;
            BindingContext = this;
        }

        public string Message { get; private set; }

        public void Hide()
        {
            Close();
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
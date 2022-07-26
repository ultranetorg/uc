using CommunityToolkit.Maui.Views;

namespace UC.Net.Node.MAUI.Popups
{
    public partial class NoNetworkPopup : Popup
    {
        public NoNetworkPopup()
        {
            InitializeComponent();
			Size = PopupSizeConstants.AutoCompleteControl;
        }

        public void Hide()
        {
            Close();
        }
    }
}
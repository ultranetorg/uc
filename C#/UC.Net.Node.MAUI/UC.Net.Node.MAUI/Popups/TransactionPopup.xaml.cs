namespace UC.Net.Node.MAUI.Popups
{
    public partial class TransactionPopup : Popup
    {
		private static TransactionPopup popup;
        public Transaction Transaction { get; }
        public Wallet Wallet { get; }

        public TransactionPopup(Transaction transaction)
        {
            InitializeComponent();
            Transaction = transaction;
            Wallet = transaction.Wallet;
            BindingContext = this;
        }

        public void Hide()
        {
			Close();
        }

		public static async Task Show(Transaction transaction)
		{
			popup = new TransactionPopup(transaction);
			await App.Current.MainPage.ShowPopupAsync(popup);
		}
	}
}
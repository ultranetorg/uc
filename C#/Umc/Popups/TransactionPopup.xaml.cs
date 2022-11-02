namespace UC.Umc.Popups;

public partial class TransactionPopup : Popup
{
	private static TransactionPopup popup;
    public TransactionViewModel Transaction { get; }
    public AccountViewModel Account { get; }

    public TransactionPopup(TransactionViewModel transaction)
    {
        InitializeComponent();
        Transaction = transaction;
        Account = transaction.Account;
        BindingContext = this;
    }

    public void Hide()
    {
	Close();
    }

	public static async Task Show(TransactionViewModel transaction)
	{
		popup = new TransactionPopup(transaction);
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}
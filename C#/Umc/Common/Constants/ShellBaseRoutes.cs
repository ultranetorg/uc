using UC.Umc.Pages;

namespace UC.Umc.Constants;

internal static class ShellBaseRoutes
{
    public const string DASHBOARD = nameof(DashboardPage);
    public const string NETWORK = nameof(NetworkPage);
    public const string HELP = nameof(HelpPage);
    public const string ABOUT = nameof(AboutPage);
	
    public const string ACCOUNTS = nameof(ManageAccountsPage);
    public const string ACCOUNT_DETAILS = nameof(AccountDetailsPage);
    public const string CREATE_ACCOUNT = nameof(CreateAccountPage);
    public const string RESTORE_ACCOUNT = nameof(RestoreAccountPage);
    public const string DELETE_ACCOUNT = nameof(DeleteAccountPage);
    public const string PRIVATE_KEY = nameof(PrivateKeyPage);
	
    public const string AUTHORS = nameof(AuthorsPage);
    public const string AUTHOR_DETAILS = nameof(AuthorDetailsPage);
    public const string AUTHOR_REGISTRATION = nameof(AuthorRegistrationPage);
    public const string AUTHOR_RENEWAL = nameof(AuthorRenewalPage);
	public const string AUTHOR_TRANSFER = nameof(AuthorTransferPage);
    public const string MAKE_BID = nameof(MakeBidPage);
    public const string PRODUCTS = nameof(ProductsPage);
    public const string PRODUCT_SEARCH = nameof(ProductSearchPage);
	public const string PRODUCT_REGISTRATION = nameof(ProductRegistrationPage);
	public const string PRODUCT_TRANSFER = nameof(ProductTransferPage);

	public const string SEND = nameof(SendPage);
	public const string TRANSACTIONS = nameof(TransactionsPage);
    public const string TRANSFER = nameof(ETHTransferPage);
	public const string COMPLETED_TRANSFERS = nameof(TransferCompletePage);
	public const string UNFINISHED_TRANSFERS = nameof(UnfinishTransferPage);
}

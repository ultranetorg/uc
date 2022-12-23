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
	// public const string AUTHOR_TRANSFER =nameof(AuthorTransferPage);
    public const string MAKE_BID = nameof(MakeBidPage);
    public const string PRODUCTS = nameof(ProductsPage); // or ProductsBPage?
    public const string PRODUCTS_SEARCH = nameof(ProductSearchPage);

    public const string SEND = nameof(SendPage);
    public const string TRANSACTIONS = nameof(TransactionsPage);
    public const string TRANSFER = nameof(ETHTransferPage);
	
	internal static void InitializeRouting()
	{
		Routing.RegisterRoute(ACCOUNT_DETAILS, typeof(AccountDetailsPage));
		Routing.RegisterRoute(CREATE_ACCOUNT, typeof(CreateAccountPage));
		Routing.RegisterRoute(RESTORE_ACCOUNT, typeof(RestoreAccountPage));
		Routing.RegisterRoute(DELETE_ACCOUNT, typeof(DeleteAccountPage));
		Routing.RegisterRoute(PRIVATE_KEY, typeof(PrivateKeyPage));

		Routing.RegisterRoute(AUTHOR_DETAILS, typeof(AuthorDetailsPage));
		Routing.RegisterRoute(AUTHOR_REGISTRATION, typeof(AuthorRegistrationPage));
		Routing.RegisterRoute(AUTHOR_RENEWAL, typeof(AuthorRenewalPage));
		//Routing.RegisterRoute(AUTHOR_TRANSFER, typeof(AuthorTransferPage));
		Routing.RegisterRoute(MAKE_BID, typeof(MakeBidPage));
		Routing.RegisterRoute(PRODUCTS_SEARCH, typeof(ProductSearchPage));

		Routing.RegisterRoute(SEND, typeof(SendPage));

		Routing.RegisterRoute(ABOUT, typeof(AboutPage));
	}
}

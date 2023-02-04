namespace UC.Umc.Popups;

public partial class TransactionPopup : Popup
{
	TransactionDetailsViewModel Vm => BindingContext as TransactionDetailsViewModel;

    public TransactionPopup(TransactionViewModel transaction)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<TransactionDetailsViewModel>();
        Vm.Transaction = transaction;
        Vm.Account = transaction.Account;
		Vm.Popup = this;
    }
}
using UO.Mobile.UUC.Models.Transactions;
using UO.Mobile.UUC.Services.Transactions;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels;

public class TransactionsViewModel : BaseViewModel
{
    private ObservableCollection<Transaction> _transactions;

    private readonly ITransactionsService _transactionsService;

    public TransactionsViewModel(ITransactionsService transactionsService)
    {
        _transactionsService = transactionsService;

        Fetch();
    }

    private async void Fetch()
    {
        IsBuisy = true;
        Transactions = await _transactionsService.GetLastAsync(10);
        IsBuisy = false;
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set
        {
            _transactions = value;
            OnPropertyChanged(nameof(Transactions));
        }
    }
}

using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Models.Transactions;

namespace UO.Mobile.UUC.Services;

public interface IServicesMockData
{
    public IList<Account> Accounts { get; set; }

    public IList<Author> Authors { get; }

    public IList<Product> Products { get; }

    public IEnumerable<Transaction> Transactions { get; }
}

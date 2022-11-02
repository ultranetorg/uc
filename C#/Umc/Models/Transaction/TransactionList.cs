namespace UC.Net.Node.MAUI.Models;

public class TransactionList: CustomCollection<TransactionViewModel>
{
    public string Header { get; set; }
    public CustomCollection<TransactionViewModel> Transactions => this;

    public TransactionList(string header)
    {
        Header = header;
    }
}

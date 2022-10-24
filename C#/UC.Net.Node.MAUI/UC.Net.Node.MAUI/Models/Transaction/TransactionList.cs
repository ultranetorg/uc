namespace UC.Net.Node.MAUI.Models;

public class TransactionList: CustomCollection<Transaction>
{
    public string Header { get; set; }
    public CustomCollection<Transaction> Transactions => this;

    public TransactionList(string header)
    {
        Header = header;
    }
}

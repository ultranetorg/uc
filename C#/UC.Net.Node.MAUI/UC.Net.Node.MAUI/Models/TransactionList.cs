namespace UC.Net.Node.MAUI.Models;

public class TransactionList: CustomCollection<Transaction>
{
    public string Head { get; set; }
    public CustomCollection<Transaction> Transactions => this;

    public TransactionList(string head)
    {
        Head = head;
    }
}

namespace UC.Net.Node.MAUI.Models;

public class TransactionList: CustomCollection<Transaction>
{
    public TransactionList(string head)
    {
        Head = head;
    }
    public string Head { get; set; }
    public CustomCollection<Transaction> Transactions => this;
}

namespace UO.Mobile.UUC.Models.Transactions;

public class Transaction
{
    public int Id { get; set; }

    public TransactionStatus Status { get; set; }

    public Account Account { get; set; }
}

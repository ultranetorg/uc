namespace UC.Net.Node.MAUI.Models;

public class Transaction
{
	// TBR
	public Guid Id { get; set; }
    public Wallet Wallet { get; set; }  
    public string FromId { get; set; }
    public string ToId { get; set; }
    public string Name { get; set; }
    public double USD { get; set; }
    public int Unt { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime Date { set; get; } = DateTime.Now;
    public string Hash { get; internal set; }
    public Account Account { get; set; }
}

public enum TransactionStatus
{
	None,
    Pending,
	Sent,
	Received,
	Failed,
}

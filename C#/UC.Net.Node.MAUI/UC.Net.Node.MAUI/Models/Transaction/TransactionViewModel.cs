namespace UC.Net.Node.MAUI.Models;

public class TransactionViewModel
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
    public DateTime Date { get; set; } = DateTime.Now;
    public string Hash { get; internal set; }
    public AccountViewModel Account { get; set; }

	public string DisplayDetails => $"{Unt}, {FromId} -> {ToId}";
}

public enum TransactionStatus
{
	None,
    Pending,
	Sent,
	Received,
	Failed,
}

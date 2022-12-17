namespace UC.Umc.Models;

public class TransactionViewModel
{
	// will be replaced with Transaction
	public Guid					Id { get; set; }
    public string				FromId { get; set; } // account address
    public string				ToId { get; set; } // account address
    public string				Name { get; set; }
    public decimal				Unt { get; set; }
    public TransactionStatus	Status { get; set; }
    public DateTime				Date { get; set; }
    public string				Hash { get; internal set; }
    public AccountViewModel		Account { get; set; }
	
	// lets say 1 unts = $1 unless we can recieve rate
    public decimal				USD => Unt;
	public string				DisplayDetails => $"{Unt}, {FromId} -> {ToId}";

	public TransactionViewModel()
	{
		Date = DateTime.Now;
	}
}

public enum TransactionStatus
{
	None,
    Pending,
	Sent,
	Received,
	Failed,
}

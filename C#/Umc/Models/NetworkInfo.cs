namespace UC.Umc.Models;

public class NetworkInfo
{
	public int NodesCount { get; set; }
	public int ActiveUsers { get; set; }
	public string Bandwidth { get; set; }
	public DateTime LastBlockDate { get; set; }
	public string RoundNumber { get; set; }

	public string DisplayBlockDate => LastBlockDate.ToString("dd MMM yyyy HH:mm");
}

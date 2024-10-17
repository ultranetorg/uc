namespace UC.DomainModels;

public class RoundGenerator
{
	public string Id { get; set; } = null!;

	public string Address { get; set; } = null!;

	public string[] BaseRdcIPs { get; set; } = [];

	public int CastingSince { get; set; }
}

namespace UC.DomainModels;

public class DomainModel
{
	public string Id { get; set; } = null!;

	public string Name { get; set; } = null!;

	public string Owner { get; set; } = null!;

	public int ExpirationDay { get; set; }

	public string? ComOwner { get; set; }
	public string? OrgOwner { get; set; }
	public string? NetOwner { get; set; }

	public int FirstBidDay { get; set; }

	public string? LastWinner { get; set; }

	public long LastBid { get; set; }
	public int LastBidDay { get; set; }

	public short SpaceReserved { get; set; }
	public short SpaceUsed { get; set; }

	public Uccs.Rdn.DomainChildPolicy ParentPolicy { get; set; }

	public IEnumerable<DomainResourceModel> Resources { get; set; } = null!;
}

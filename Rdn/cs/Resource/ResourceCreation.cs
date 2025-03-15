namespace Uccs.Rdn;

public class ResourceCreation : RdnOperation
{
	public Ura					Address { get; set; }
	public ResourceChanges		Changes { get; set; }
	public ResourceData			Data { get; set; }

	public override bool		IsValid(Mcv mcv) => (!Changes.HasFlag(ResourceChanges.SetData) || (Data.Value.Length <= ResourceData.LengthMax)) &&
													(!Changes.HasFlag(ResourceChanges.NullData));
	public override string		Description => $"{Address}, [{Changes}]{(Data == null ? null : ", Data=" + Data)}";

	public ResourceCreation()
	{
	}

	public ResourceCreation(Ura resource, ResourceData data, bool seal)
	{
		Address = resource;
		Data = data;

		if(Data != null)	Changes |= ResourceChanges.SetData;
		if(seal)			Changes |= ResourceChanges.Seal;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Address	= reader.Read<Ura>();
		Changes	= (ResourceChanges)reader.ReadByte();

		if(Changes.HasFlag(ResourceChanges.SetData))	Data = reader.Read<ResourceData>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Address);
		writer.Write((byte)Changes);

		if(Changes.HasFlag(ResourceChanges.SetData))	writer.Write(Data);
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		if(RequireDomainAccess(round, Address.Domain, out var d) == false)
			return;

		var r = round.Mcv.Resources.Find(Address, round.Id);
				
		if(r != null)
		{
			Error = AlreadyExists;
			return;
		}

		r = round.AffectResource(d, Address.Resource);

		if(Changes.HasFlag(ResourceChanges.SetData))
		{
			r.Data		= Data;
			r.Flags		|= ResourceFlags.Data;
			r.Updated	= round.ConsensusTime;
		}

		if(Changes.HasFlag(ResourceChanges.Seal))
		{
			r.Flags	|= ResourceFlags.Sealed;

			PayForForever(mcv.Net.EntityLength + r.Length);
		}
		else
		{	
			d = round.AffectDomain(d.Id);
			Allocate(round, Signer, d, mcv.Net.EntityLength + r.Length);
		}
	}
}
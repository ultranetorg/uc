namespace Uccs.Rdn;

public class ResourceLinkDeletion : RdnOperation
{
	public EntityId	Source { get; set; }
	public EntityId	Destination { get; set; }
	
	public override string	Description => $"Source={Source}, Destination={Destination}";
	public override bool	IsValid(Mcv mcv) => true;

	public ResourceLinkDeletion()
	{
	}

	public ResourceLinkDeletion(EntityId source, EntityId destination)
	{
		Source = source;
		Destination = destination;
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write(Destination);
	}
	
	public override void ReadConfirmed(BinaryReader reader)
	{
		Source	= reader.Read<EntityId>();
		Destination	= reader.Read<EntityId>();
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		if(RequireSignerResource(round, Source, out var sd, out var sr) == false)
			return;

		if(RequireResource(round, Destination, out var dd, out var dr) == false)
			return;

		var l = sr.Outbounds.FirstOrDefault(i => i.Destination == dr.Id);

		if(l == null)
		{
			Error = NotFound;
			return;
		}

		if(l.Flags.HasFlag(ResourceLinkFlag.Sealed))
		{
			Error = Sealed;
			return;
		}

		sr = round.AffectResource(sd, sr.Address.Resource);
		sr.RemoveOutbound(dr.Id);

		sd = round.AffectDomain(sd.Id);
		Free(round, Signer, sd, Mcv.EntityLength);

		dr = round.AffectResource(dd, dr.Address.Resource);
		dr.RemoveInbound(sr.Id);
	}
}

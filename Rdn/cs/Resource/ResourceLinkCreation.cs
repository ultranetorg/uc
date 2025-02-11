namespace Uccs.Rdn;

public class ResourceLinkCreation : RdnOperation
{
	public EntityId				Source { get; set; }
	public EntityId				Destination { get; set; }
	public ResourceLinkChanges	Changes  { get; set; }
	
	public override string	Description => $"Source={Source}, Destination={Destination}";
	public override bool	IsValid(Mcv mcv) => true;

	public ResourceLinkCreation()
	{
	}

	public ResourceLinkCreation(bool seal)
	{
		if(seal)
			Changes |= ResourceLinkChanges.Seal;
	}

	public ResourceLinkCreation(EntityId source, EntityId destination)
	{
		Source = source;
		Destination = destination;
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write(Destination);
		writer.Write((byte)Changes);
	}
	
	public override void ReadConfirmed(BinaryReader reader)
	{
		Source		= reader.Read<EntityId>();
		Destination	= reader.Read<EntityId>();
		Changes		= (ResourceLinkChanges)reader.ReadByte();
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		if(RequireSignerResource(round, Source, out var sd, out var sr) == false)
			return;

		if(RequireResource(round, Destination, out var dd, out var dr) == false)
			return;

		sr = round.AffectResource(sd, sr.Address.Resource);
		sr.AffectOutbound(dr.Id);

		dr = round.AffectResource(dd, dr.Address.Resource);
		dr.AffectInbound(sr.Id);

		if(Changes.HasFlag(ResourceLinkChanges.Seal))
		{
			if(!sr.Flags.HasFlag(ResourceFlags.Sealed) || !dr.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = NotSealed;
				return;
			}

			PayForForever(Mcv.EntityLength);
		}
		else
		{	
			sd = round.AffectDomain(sd.Id);
			Allocate(round, sd, Mcv.EntityLength);
		}
	}
}

namespace Uccs.Rdn;

public class ResourceLinkCreation : RdnOperation
{
	public AutoId				Source { get; set; }
	public AutoId				Destination { get; set; }
	public ResourceLinkChanges	Changes  { get; set; }
	
	public override string		Explanation => $"Source={Source}, Destination={Destination}";
	public override bool		IsValid(McvNet net) => true;

	public ResourceLinkCreation()
	{
	}

	public ResourceLinkCreation(bool seal)
	{
		if(seal)
			Changes |= ResourceLinkChanges.Seal;
	}

	public ResourceLinkCreation(AutoId source, AutoId destination)
	{
		Source = source;
		Destination = destination;
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write(Destination);
		writer.Write((byte)Changes);
	}
	
	public override void Read(BinaryReader reader)
	{
		Source		= reader.Read<AutoId>();
		Destination	= reader.Read<AutoId>();
		Changes		= (ResourceLinkChanges)reader.ReadByte();
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerResource(execution, Source, out var sd, out var sr) == false)
			return;

		if(RequireResource(execution, Destination, out var dd, out var dr) == false)
			return;

		sr = execution.Resources.Affect(sd, sr.Address.Resource);
		sr.AffectOutbound(dr.Id);

		dr = execution.Resources.Affect(dd, dr.Address.Resource);
		dr.AffectInbound(sr.Id);

		if(Changes.HasFlag(ResourceLinkChanges.Seal))
		{
			if(!sr.Flags.HasFlag(ResourceFlags.Sealed) || !dr.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = NotSealed;
				return;
			}

			PayForForever(execution.Net.EntityLength);
		}
		else
		{	
			sd = execution.Domains.Affect(sd.Id);
			Allocate(execution, Signer, sd, execution.Net.EntityLength);
		}
	}
}

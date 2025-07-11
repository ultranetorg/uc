namespace Uccs.Rdn;

public class ResourceLinkDeletion : RdnOperation
{
	public AutoId	Source { get; set; }
	public AutoId	Destination { get; set; }
	
	public override string	Explanation => $"Source={Source}, Destination={Destination}";
	public override bool	IsValid(McvNet net) => true;

	public ResourceLinkDeletion()
	{
	}

	public ResourceLinkDeletion(AutoId source, AutoId destination)
	{
		Source = source;
		Destination = destination;
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write(Destination);
	}
	
	public override void Read(BinaryReader reader)
	{
		Source	= reader.Read<AutoId>();
		Destination	= reader.Read<AutoId>();
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerResource(execution, Source, out var sd, out var sr) == false)
			return;

		if(RequireResource(execution, Destination, out var dd, out var dr) == false)
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

		sr = execution.Resources.Affect(sd, sr.Address.Resource);
		sr.RemoveOutbound(dr.Id);

		sd = execution.Domains.Affect(sd.Id);
		execution.Free(Signer, sd, execution.Net.EntityLength);

		dr = execution.Resources.Affect(dd, dr.Address.Resource);
		dr.RemoveInbound(sr.Id);
	}
}

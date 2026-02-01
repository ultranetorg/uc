namespace Uccs.Rdn;

public class ResourceLinkDeletion : RdnOperation
{
	public AutoId	Source { get; set; }
	public int		Index { get; set; }
	
	public override string	Explanation => $"Source={Source}, Index={Index}";
	public override bool	IsValid(McvNet net) => true;

	public ResourceLinkDeletion()
	{
	}

	public ResourceLinkDeletion(AutoId source, int destination)
	{
		Source = source;
		Index = Index;
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Source);
		writer.Write7BitEncodedInt(Index);
	}
	
	public override void Read(BinaryReader reader)
	{
		Source	= reader.Read<AutoId>();
		Index	= reader.Read7BitEncodedInt();
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerResource(execution, Source, out var sd, out var sr) == false)
			return;

		if(Index >= sr.Outbounds.Length)
		{
			Error = NotFound;
			return;
		}

		var l = sr.Outbounds[Index];

		if(l.Type.HasFlag(ResourceLinkType.Dependency) && sr.IsLocked(execution)) /// a resource with dependent ones cant change its own dependencies
		{
			Error = Locked;
			return;
		}

		sr = execution.Resources.Affect(Source);
		sr.RemoveOutbound(l.Destination);

		sd = execution.Domains.Affect(sd.Id);
		execution.Free(User, sd, execution.Net.EntityLength);

		var dr = execution.Resources.Affect(l.Destination);
		dr.RemoveInbound(Source);

		execution.PayOperationEnergy(User);
	}
}

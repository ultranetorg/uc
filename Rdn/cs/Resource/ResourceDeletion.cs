namespace Uccs.Rdn;

public class ResourceDeletion : RdnOperation
{
	public AutoId				Resource { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Resource={Resource}";

	public ResourceDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Resource = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Resource);
	}

	public override void Execute(RdnExecution execution)
	{
		if(RequireSignerResource(execution, Resource, out var d, out var r) == false)
			return;

		if(r.Flags.HasFlag(ResourceFlags.Sealed))
		{
			Error = Sealed;
			return;
		}

		d = execution.Domains.Affect(d.Id);
		execution.Resources.Affect(Resource).Deleted = true;

		execution.Free(Signer, d, execution.Net.EntityLength + r.Length);

		foreach(var i in r.Outbounds)
		{
			var dr = execution.Resources.Find(i.Destination);

			dr = execution.Resources.Affect(d, dr.Address.Resource);
			dr.RemoveInbound(r.Id);

			execution.Free(Signer, d, execution.Net.EntityLength);
		}

		foreach(var i in r.Inbounds ?? [])
		{
			var sr = execution.Resources.Find(i);

			sr = execution.Resources.Affect(d, sr.Address.Resource);
			sr.RemoveOutbound(r.Id);
		}
	}
}
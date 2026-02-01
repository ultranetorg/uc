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

		if(r.IsLocked(execution))
		{
			Error = Locked;
			return;
		}

		d = execution.Domains.Affect(d.Id);
		execution.Resources.Affect(Resource).Deleted = true;

		execution.Free(User, d, execution.Net.EntityLength + r.DataLength);

		foreach(var i in r.Outbounds)
		{
			var dr = execution.Resources.Affect(i.Destination);
			dr.RemoveInbound(r.Id);

			execution.Free(User, d, execution.Net.EntityLength);
		}

		foreach(var i in r.Inbounds ?? [])
		{
			var sr = execution.Resources.Affect(i);
			sr.RemoveOutbound(r.Id);
		}
	}
}
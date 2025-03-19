namespace Uccs.Rdn;

public class ResourceDeletion : RdnOperation
{
	public EntityId				Resource { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public ResourceDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Resource = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Resource);
	}

	public override void Execute(RdnExecution execution, RdnRound round)
	{
		if(RequireSignerResource(execution, Resource, out var d, out var r) == false)
			return;

		if(r.Flags.HasFlag(ResourceFlags.Sealed))
		{
			Error = Sealed;
			return;
		}

		d = execution.AffectDomain(d.Id);
		execution.AffectResource(Resource).Deleted = true;

		Free(execution, Signer, d, execution.Net.EntityLength + r.Length);

		foreach(var i in r.Outbounds)
		{
			var dr = execution.FindResource(i.Destination);

			dr = execution.AffectResource(d, dr.Address.Resource);
			dr.RemoveInbound(r.Id);

			Free(execution, Signer, d, execution.Net.EntityLength);
		}

		foreach(var i in r.Inbounds ?? [])
		{
			var sr = execution.FindResource(i);

			sr = execution.AffectResource(d, sr.Address.Resource);
			sr.RemoveOutbound(r.Id);
		}
	}
}
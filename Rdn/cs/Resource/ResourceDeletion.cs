namespace Uccs.Rdn;

public class ResourceDeletion : RdnOperation
{
	public ResourceId			Resource { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Id}";

	public ResourceDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Resource = reader.Read<ResourceId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Resource);
	}

	public override void Execute(RdnMcv mcv, RdnRound round)
	{
		if(RequireSignerResource(round, Resource, out var d, out var r) == false)
			return;

		if(r.Flags.HasFlag(ResourceFlags.Sealed))
		{
			Error = Sealed;
			return;
		}

		round.AffectResource(Resource).Deleted = true;

		Free(d, r.Length);

		foreach(var i in r.Outbounds)
		{
			var dr = mcv.Resources.Find(i.Destination, round.Id);

			dr = round.AffectResource(d, dr.Address.Resource);
			dr.RemoveInbound(r.Id);

			Free(d, Mcv.EntityLength);
		}

		foreach(var i in r.Inbounds ?? [])
		{
			var sr = mcv.Resources.Find(i, round.Id);

			sr = round.AffectResource(d, sr.Address.Resource);
			sr.RemoveOutbound(r.Id);
		}
	}
}
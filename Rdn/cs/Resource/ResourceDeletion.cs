namespace Uccs.Rdn
{
	public class ResourceDeletion : RdnOperation
	{
		public new ResourceId		Id { get; set; }

		public override bool		IsValid(Mcv mcv) => true;
		public override string		Description => $"{Id}";

		public ResourceDeletion()
		{
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Id = reader.Read<ResourceId>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Id);
		}

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			if(RequireSignerResource(round, Id, out var d, out var r) == false)
				return;

			if(r.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = Sealed;
				return;
			}

			var s = round.AffectSite(Id.DomainId);
			s.DeleteResource(r);

			Free(d, r.Length);

			foreach(var i in r.Outbounds)
			{
				var dr = mcv.Sites.FindResource(i.Destination, round.Id);

				dr = round.AffectSite(dr.Id.DomainId).AffectResource(d, dr.Address.Resource);
				dr.RemoveInbound(r.Id);

				Free(d, Mcv.EntityLength);
			}

			foreach(var i in r.Inbounds ?? [])
			{
				var sr = mcv.Sites.FindResource(i, round.Id);

				sr = round.AffectSite(sr.Id.DomainId).AffectResource(d, sr.Address.Resource);
				sr.RemoveOutbound(r.Id);
			}
		}
	}
}
using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
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
			if(Require(round, Signer, Id, out var a, out var r) == false)
				return;

			if(r.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = Sealed;
				return;
			}

			a = round.AffectDomain(Id.DomainId);
			a.DeleteResource(r);

			Free(a, r.Length);

			foreach(var i in r.Outbounds)
			{
				var dr = mcv.Domains.FindResource(i.Destination, round.Id);

				dr = round.AffectDomain(dr.Address.Domain).AffectResource(dr.Address.Resource);
				dr.RemoveInbound(r.Id);

				Free(a, Mcv.EntityLength);
			}

			foreach(var i in r.Inbounds ?? [])
			{
				var sr = mcv.Domains.FindResource(i, round.Id);

				sr = round.AffectDomain(sr.Address.Domain).AffectResource(sr.Address.Resource);
				sr.RemoveOutbound(r.Id);
			}
		}
	}
}
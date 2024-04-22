using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceDeletion : Operation
	{
		public ResourceAddress		Resource { get; set; }

		public override bool		Valid => true;
		public override string		Description => $"{Resource}";

		public ResourceDeletion()
		{
		}

		public ResourceDeletion(ResourceAddress resource)
		{
			Resource = resource;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, Signer, Resource, out var a, out var r) == false)
				return;

			if(r.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = Sealed;
				return;
			}

			a = Affect(round, Resource.Domain);
			a.DeleteResource(r);

			Free(a, r.Length);

			foreach(var i in r.Outbounds)
			{
				var dr = mcv.Domains.FindResource(i.Destination, round.Id);

				dr = Affect(round, dr.Address.Domain).AffectResource(dr.Address.Resource);
				dr.RemoveInbound(r.Id);

				Free(a, Mcv.EntityLength);
			}

			foreach(var i in r.Inbounds ?? [])
			{
				var sr = mcv.Domains.FindResource(i, round.Id);

				sr = Affect(round, sr.Address.Domain).AffectResource(sr.Address.Resource);
				sr.RemoveOutbound(r.Id);
			}
		}
	}
}
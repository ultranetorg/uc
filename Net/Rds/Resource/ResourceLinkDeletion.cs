using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceLinkDeletion : RdsOperation
	{
		public ResourceId	Source { get; set; }
		public ResourceId	Destination { get; set; }
		
		public override string	Description => $"Source={Source}, Destination={Destination}";
		public override bool	IsValid(Mcv mcv) => true;

		public ResourceLinkDeletion()
		{
		}

		public ResourceLinkDeletion(ResourceId source, ResourceId destination)
		{
			Source = source;
			Destination = destination;
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Source);
			writer.Write(Destination);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Source	= reader.Read<ResourceId>();
			Destination	= reader.Read<ResourceId>();
		}

		public override void Execute(Rds mcv, RdsRound round)
		{
			if(Require(round, Signer, Source, out var sd, out var sr) == false)
				return;

			if(Require(round, null, Destination, out var dd, out var dr) == false)
				return;

			var l = sr.Outbounds.First(i => i.Destination == dr.Id);

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

			sd = round.AffectDomain(sd.Id);
			sr = sd.AffectResource(sr.Address.Resource);
			sr.RemoveOutbound(dr.Id);
			Free(sd, Mcv.EntityLength);

			dd = round.AffectDomain(dd.Id);
			dr = dd.AffectResource(dr.Address.Resource);
			dr.RemoveInbound(sr.Id);
		}
	}
}

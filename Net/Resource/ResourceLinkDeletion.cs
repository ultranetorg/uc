using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceLinkDeletion : Operation
	{
		public ResourceAddress	Source { get; set; }
		public ResourceAddress	Destination { get; set; }
		
		public override string	Description => $"Source={Source}, Destination={Destination}";
		public override bool	Valid => true;

		public ResourceLinkDeletion()
		{
		}

		public ResourceLinkDeletion(ResourceAddress source, ResourceAddress destination)
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
			Source	= reader.Read<ResourceAddress>();
			Destination	= reader.Read<ResourceAddress>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, Signer, Source, out var sa, out var sr) == false)
				return;

			if(Require(round, null, Destination, out var da, out var dr) == false)
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

			sa = Affect(round, Source.Author);
			sr = sa.AffectResource(Source.Resource);
			sr.RemoveOutbound(dr.Id);
			Free(sa, Mcv.EntityLength);

			da = Affect(round, Destination.Author);
			dr = da.AffectResource(Destination.Resource);
			dr.RemoveInbound(sr.Id);
		}
	}
}

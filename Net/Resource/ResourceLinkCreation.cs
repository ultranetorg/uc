using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceLinkCreation : Operation
	{
		public ResourceAddress	Resource { get; set; }
		public ResourceAddress	To { get; set; }
		//public ResourceData		Data { get; set; }
		
		public override string	Description => $"Resource={Resource}, To={To}";
		public override bool	Valid => true;

		public ResourceLinkCreation()
		{
		}

		public ResourceLinkCreation(ResourceAddress resource, ResourceAddress to)
		{
			Resource = resource;
			To = to;
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(To);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			To		 = reader.Read<ResourceAddress>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var s = mcv.Accounts.Find(Signer, round.Id);
					
			if(s == null)
			{
				Error = NotFound;
				return;
			}

			if(Require(round, null, Resource, out var a, out var r) == false)
				return;

			if(Require(round, Signer, To, out var la, out var lr) == false)
				return;

			a = Affect(round, Resource.Author);
			r = a.AffectResource(Resource.Resource);
			r.AffectLink(lr.Id);

			//r.Links ??= [];
			//r.Links = r.Links.Append(to.Id).ToArray();

			//PayForEntity(round, Time.FromYears(10));
			//PayForBytes(round, Data.Value.Length, Time.FromYears(10));
		}
	}
}

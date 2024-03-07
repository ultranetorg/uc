using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceRelationCreation : Operation
	{
		public ResourceAddress	Resource { get; set; }
		public ResourceData		Data { get; set; }
		
		public override string	Description => $"Resource={Resource}, Data={Data}";
		public override bool	Valid => true;

		public ResourceRelationCreation()
		{
		}

		public ResourceRelationCreation(ResourceAddress resource, ResourceData data)
		{
			Resource = resource;
			Data = data;
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(Data);
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Data	 = reader.Read<ResourceData>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Require(round, Resource, out var a, out var r) == false)
				return;

			//var rr = r.Relations.FirstOrDefault(i => mcv.Authors.FindEntry(new EntityId(i.Resource.Ci, i.Resource.Ai)).Resources.First(j => j.Id.Ri == i.Resource.Ri).Address == Related);
			//
			//if(rr != null)
			//{
			//	Error = AlreadyExists;
			//	return;
			//}

			var s = mcv.Accounts.Find(Signer, round.Id);
					
			if(s == null)
			{
				Error = NotFound;
				return;
			}

			a = Affect(round, Resource.Author);
			r = a.AffectResource(Resource.Resource);

			r.Relations ??= [];
			r.Relations = r.Relations.Append(new ResourceRelation {Owner = s.Id, Data = Data}).ToArray();

			PayForEntity(round, Time.FromYears(10));
			PayForBytes(round, Data.Value.Length, Time.FromYears(10));
		}
	}
}

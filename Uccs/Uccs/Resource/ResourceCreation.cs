using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class ResourceCreation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public string				Parent { get; set; }

		public override bool		Valid => !Flags.HasFlag(ResourceFlags.Data) || (Data.Value.Length <= ResourceData.LengthMax);
		public override string		Description => $"{Resource}, [{Flags}]{(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : ", Data=" + Data)}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(ResourceAddress resource, ResourceFlags flags, ResourceData data, string parent)
		{
			Resource = resource;
			Flags = flags;
			Data = data;
			Parent = parent;

			if(Data != null)					Flags |= ResourceFlags.Data;
			if(Parent != null)					Flags |= ResourceFlags.Child;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Flags		= (ResourceFlags)reader.ReadByte();

			if(Flags.HasFlag(ResourceFlags.Data))		Data = reader.Read<ResourceData>();
			if(Flags.HasFlag(ResourceFlags.Child))		Parent = reader.ReadUtf8();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Flags);

			if(Flags.HasFlag(ResourceFlags.Data))		writer.Write(Data);
			if(Flags.HasFlag(ResourceFlags.Child))		writer.WriteUtf8(Parent);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var a = mcv.Authors.Find(Resource.Author, round.Id);

			if(a == null)
			{
				Error = NotFound;
				return;
			}

			if(a.Owner != Signer)
			{
				Error = NotOwner;
				return;
			}

			if(Author.IsExpired(a, round.ConsensusTime))
			{
				Error = Expired;
				return;
			}

			var e = a.Resources.FirstOrDefault(i => i.Address == Resource);
					
			if(e != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = Affect(round, Resource.Author);
			var r = a.AffectResource(Resource.Resource);

			r.Flags	= Flags;

			PayForEntity(round, a.Expiration - round.ConsensusTime);
			
			if(Flags.HasFlag(ResourceFlags.Child))
			{
				var i = Array.FindIndex(a.Resources, i => i.Address.Resource == Parent);
	
				if(i == -1)
				{
					Error = NotFound;
					return;
				}
	
				var p = a.AffectResource(Parent);
				p.Resources = p.Resources.Append(r.Id.Ri).ToArray();
			}
						
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				r.Updated	= round.ConsensusTime;
				r.Data		= Data;

				if(Data != null)
				{
					if(a.SpaceReserved < a.SpaceUsed + r.Data.Value.Length)
					{
						Expand(round, a, r.Data.Value.Length);
					}
				} 
			}
		}
	}
}
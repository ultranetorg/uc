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

		public override bool		Valid => !Flags.HasFlag(ResourceFlags.Data) || (Data.Value.Length <= ResourceData.LengthMax);
		public override string		Description => $"{Resource}, [{Flags}]{(Data == null ? null : ", Data=" + Data)}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(ResourceAddress resource, ResourceFlags flags, ResourceData data)
		{
			Resource = resource;
			Flags = flags;
			Data = data;

			if(Data != null)	Flags |= ResourceFlags.Data;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Flags		= (ResourceFlags)reader.ReadByte();

			if(Flags.HasFlag(ResourceFlags.Data))	Data = reader.Read<ResourceData>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Flags);

			if(Flags.HasFlag(ResourceFlags.Data))	writer.Write(Data);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(RequireAuthor(round, Signer, Resource.Author, out var a) == false)
				return;

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
			
			//if(Flags.HasFlag(ResourceFlags.Child))
			//{
			//	var i = Array.FindIndex(a.Resources, i => i.Address.Resource == Parent);
			//
			//	if(i == -1)
			//	{
			//		Error = NotFound;
			//		return;
			//	}
			//
			//	var p = a.AffectResource(Parent);
			//	p.Resources = p.Resources.Append(r.Id.Ri).ToArray();
			//}
						
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
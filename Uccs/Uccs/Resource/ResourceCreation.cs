using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;

namespace Uccs.Net
{
	public class ResourceCreation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Initials { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceType			Type { get; set; }
		public byte[]				Data { get; set; }
		public string				Parent { get; set; }
		//public Money				AnalysisFee { get; set; }

		public override bool		Valid => (Flags & ResourceFlags.Unchangables) == 0
												&& (!Initials.HasFlag(ResourceChanges.Data)	|| Initials.HasFlag(ResourceChanges.Data) && Data.Length <= Net.Resource.DataLengthMax)
											;
		
		public override string		Description => $"{Resource}, [{Initials}], [{Flags}], {Type}{(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : ", Data=" + Hex.ToHexString(Data))}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(ResourceAddress resource, ResourceFlags flags, ResourceType type, byte[] data, string parent)
		{
			Resource = resource;
			Flags = flags;
			Type = type;

			Initials |= (ResourceChanges.Flags|ResourceChanges.Type);

			if(data != null && data.Length > 0)
			{
				Data = data;
				Initials |= ResourceChanges.Data;
			}

			if(parent != null)
			{
				Parent = parent;
				Initials |= ResourceChanges.Parent;
			}
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Initials	= (ResourceChanges)reader.ReadByte();
			Flags		= (ResourceFlags)reader.ReadByte();
			Type		= (ResourceType)reader.Read7BitEncodedInt();

			if(Initials.HasFlag(ResourceChanges.Data))		Data = reader.ReadBytes();
			if(Initials.HasFlag(ResourceChanges.Parent))	Parent = reader.ReadUtf8();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Initials);
			writer.Write((byte)Flags);
			writer.Write7BitEncodedInt((short)Type);

			if(Initials.HasFlag(ResourceChanges.Data))		writer.WriteBytes(Data);
			if(Initials.HasFlag(ResourceChanges.Parent))	writer.WriteUtf8(Parent);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var a = chain.Authors.Find(Resource.Author, round.Id);

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

			var e = chain.Authors.FindResource(Resource, round.Id);
					
			if(e != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = Affect(round, Resource.Author);
			var r = a.AffectResource(Resource);

			r.Flags	= r.Flags & ResourceFlags.Unchangables | Flags & ~ResourceFlags.Unchangables;
			r.Type	= Type;
			
			var y = (byte)((round.ConfirmedTime.Ticks - a.Expiration.Ticks) / Time.FromYears(1).Ticks + 1);

			PayForEnity(round, y);
			
			if(Parent != null)
			{
				r.Flags |= ResourceFlags.Child;

				var i = Array.FindIndex(a.Resources, i => i.Address.Resource == Parent);

				if(i == -1)
				{
					Error = NotFound;
					return;
				}

				var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
				p.Resources = p.Resources.Append(r.Id).ToArray();
			}
						
			if(Data != null)
			{
				r.Flags		|= ResourceFlags.Data;
				r.Data		= Data;

				if(a.SpaceReserved < a.SpaceUsed + r.Data.Length)
				{
					PayForResourceData(round, a.SpaceUsed + r.Data.Length - a.SpaceReserved, y);

					a.SpaceUsed		= (short)(a.SpaceUsed + r.Data.Length);
					a.SpaceReserved	= a.SpaceUsed;
	
				}
			}
		}
	}
}
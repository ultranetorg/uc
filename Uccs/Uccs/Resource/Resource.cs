using System;
using System.IO;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None		= 0, 
		Sealed		= 0b________1, 
		Deprecated	= 0b_______10, 
		Child		= 0b______100, 
		Data		= 0b_____1000, 

		Unchangables= Child | Data, 
	}

	public enum DataType : short
	{
		None		= 0,
		Redirect	= 1,
		IPAddress	= 2, 
		Uri			= 3,
				
		File		= 100, 
		Directory	= 101, 
		Package		= 102, 

		FirstMime	= 1000, 
	}

	[Flags]
	public enum ResourceChanges : ushort
	{
		None			= 0,
		Flags			= 0b______________1,
		//Type			= 0b_____________10,
		Data			= 0b____________100,
		Parent			= 0b___________1000,
		AddPublisher	= 0b__________10000,
		RemovePublisher	= 0b_________100000,
		Recursive		= 0b100000000000000,
	}

	public class Resource : IBinarySerializable
	{
		public const short		DataLengthMax = 8192;

		public ResourceId		Id { get; set; }
		public ResourceAddress	Address { get; set; }
		public ResourceFlags	Flags { get; set; }
		//public ResourceType	Type { get; set; }
		public byte[]			Data { get; set; }
		public int[]			Resources { get; set; } = {};

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={(Data == null ? null : ('[' + Data.Length + ']'))} Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data,
							Resources = Resources};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				writer.WriteBytes(Data);
			}

			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
		}

		public void Read(BinaryReader reader)
		{
			Flags	= (ResourceFlags)reader.ReadByte();
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				Data = reader.ReadBytes();
			}

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
		}
	}
}

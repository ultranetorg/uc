using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None			= 0, 
		Sealed			= 0b_______1, 
		Data			= 0b______10,
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
		Consil		= 103, 
		Analysis	= 104, 

		FirstMime	= 1000, 
	}

	[Flags]
	public enum ResourceChanges : byte
	{
		None			= 0,
		SetData			= 0b________1,
		NullData		= 0b_______10,
		Seal			= 0b______100,
		Recursive		= 0b1000_0000,
	}

	public class Resource : IBinarySerializable
	{
		public ResourceId			Id { get; set; }
		public ResourceAddress		Address { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public Time					Updated { get; set; }
		public ResourceId[]			Links { get; set; }

		[JsonIgnore]
		public bool					New;
		List<ResourceId>			AffectedLinks = new();

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Links={{{Links.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data?.Clone(),
							Updated = Updated,
							Links = Links,
							};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			writer.Write(Updated);
			
			if(Flags.HasFlag(ResourceFlags.Data))
				writer.Write(Data);
		
			writer.Write(Links);
		}

		public void Read(BinaryReader reader)
		{
			Flags = (ResourceFlags)reader.ReadByte();
			Updated	= reader.Read<Time>();

			if(Flags.HasFlag(ResourceFlags.Data))
				Data = reader.Read<ResourceData>();

			Links = reader.ReadArray<ResourceId>();
		}

		public void AffectLink(ResourceId to)
		{
			var i = AffectedLinks.IndexOf(to);
			
			if(i != -1)
				return;

			Links ??= [];

			if(i != -1)
			{
				Links = Links.ToArray();
			} 
			else
			{
				Links = Links.Append(to).ToArray();
			}

			AffectedLinks.Add(to);
		}
	}
}

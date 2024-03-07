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
		Child			= 0b______10, 
		Data			= 0b_____100, 
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
		Recursive		= 0b______________1,
		NotNullData		= 0b_____________10,
	}

	public class ResourceMeta : IBinarySerializable
	{
		public EntityId			Owner;
		public ResourceData		Data;

		public void Write(BinaryWriter writer)
		{
			writer.Write(Owner);
			writer.Write(Data);
		}

		public void Read(BinaryReader reader)
		{
			Owner	= reader.Read<EntityId>();
			Data	= reader.Read<ResourceData>();
		}

		public ResourceMeta Clone()
		{
			return new ResourceMeta {Owner = Owner, Data = Data.Clone()};
		}
	}

	public class Resource : IBinarySerializable
	{
		public ResourceId			Id { get; set; }
		public ResourceAddress		Address { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public Time					Updated { get; set; }
		public int[]				Resources { get; set; } = {};
		public ResourceMeta[]		Metas { get; set; }

		[JsonIgnore]
		public bool					New;
		List<ResourceMeta>			AffectedMetas = new();

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data,
							Updated = Updated,
							Resources = Resources,
							Metas = Metas?.ToArray(),
							};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			writer.Write(Updated);
			
			if(Flags.HasFlag(ResourceFlags.Data))
				writer.Write(Data);
		
			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
			writer.Write(Metas);
		}

		public void Read(BinaryReader reader)
		{
			Flags = (ResourceFlags)reader.ReadByte();
			Updated	= reader.Read<Time>();

			if(Flags.HasFlag(ResourceFlags.Data))
				Data = reader.Read<ResourceData>();

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
			Metas = reader.ReadArray<ResourceMeta>();
		}

		public ResourceMeta AffectMeta(EntityId owner, ResourceData data)
		{
			var m = AffectedMetas.Find(i => i.Owner == owner && i.Data == data);
			
			if(m != null)
				return m;

			var i = Array.FindIndex(Metas, i => i.Owner == owner && i.Data == data);

			if(i != -1)
			{
				Metas = Metas.ToArray();

				m = Metas[i].Clone();
				Metas[i] = m;
			} 
			else
			{
				m = new ResourceMeta {Owner = owner, Data = data};
				Metas = Metas.Append(m).ToArray();
			}

			AffectedMetas.Add(m);

			return m;
		}
	}
}

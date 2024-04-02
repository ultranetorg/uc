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

	public enum ResourceLinkFlag : byte
	{
		Sealed			= 0b________1,
	}

	public class ResourceLink : IBinarySerializable
	{
		public ResourceId		Destination { get; set; }
		public ResourceLinkFlag	Flags { get; set; }

		public ResourceLink Clone()
		{
			return new ResourceLink {Destination = Destination, Flags = Flags};
		}

		public void Read(BinaryReader reader)
		{
			Destination	= reader.Read<ResourceId>();
			Flags		= (ResourceLinkFlag)reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Destination);
			writer.Write((byte)Flags);
		}
	}

	public class Resource : IBinarySerializable
	{
		public ResourceId			Id { get; set; }
		public ResourceAddress		Address { get; set; }
		public ResourceFlags		Flags { get; set; }
		public ResourceData			Data { get; set; }
		public Time					Updated { get; set; }
		public ResourceLink[]		Outbounds { get; set; }
		public ResourceId[]			Inbounds { get; set; }

		[JsonIgnore]
		public bool					New;
		List<ResourceLink>			AffectedOutbounds = new();
		List<ResourceId>			AffectedInbounds = new();

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Outbounds={{{Outbounds.Length}}}, Inbounds={{{Inbounds.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Flags = Flags,
							Data = Data?.Clone(),
							Updated = Updated,
							Outbounds = Outbounds,
							Inbounds = Inbounds };
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Flags);
			writer.Write(Updated);
			
			if(Flags.HasFlag(ResourceFlags.Data))
				writer.Write(Data);
		
			writer.Write(Outbounds);
			writer.Write(Inbounds);
		}

		public void Read(BinaryReader reader)
		{
			Flags = (ResourceFlags)reader.ReadByte();
			Updated	= reader.Read<Time>();

			if(Flags.HasFlag(ResourceFlags.Data))
				Data = reader.Read<ResourceData>();

			Outbounds = reader.ReadArray<ResourceLink>();
			Inbounds = reader.ReadArray<ResourceId>();
		}

		public ResourceLink AffectOutbound(ResourceId destination)
		{
			var l = AffectedOutbounds.Find(i => i.Destination == destination);
			
			if(l != null)
				return l;

			Outbounds ??= [];

			var i = Array.FindIndex(Outbounds, i => i.Destination == destination);

			if(l != null)
			{
				Outbounds = Outbounds.ToArray();
				Outbounds[i] = l = Outbounds[i].Clone();
			} 
			else
			{
				l = new ResourceLink {Destination = destination};
				Outbounds = Outbounds.Append(l).ToArray();
			}

			AffectedOutbounds.Add(l);

			return l;
		}

		public void AffectInbound(ResourceId parent)
		{
			var i = AffectedInbounds.IndexOf(parent);
			
			if(i != -1)
				return;

			Inbounds ??= [];

			if(i != -1)
			{
				Inbounds = Inbounds.ToArray();
			} 
			else
			{
				Inbounds = Inbounds.Append(parent).ToArray();
			}

			AffectedInbounds.Add(parent);
		}
	}
}

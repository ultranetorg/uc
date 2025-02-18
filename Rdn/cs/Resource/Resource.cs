using System.Text.Json.Serialization;

namespace Uccs.Rdn;

[Flags]
public enum ResourceFlags : byte
{
	None			= 0, 
	Sealed			= 0b_______1, 
	Data			= 0b______10,
}

[Flags]
public enum ResourceChanges : byte
{
	None		= 0,
	SetData		= 0b________1,
	NullData	= 0b_______10,
	Seal		= 0b______100,
	Recursive	= 0b1000_0000,
}

public enum ResourceLinkFlag : byte
{
	None		= 0,
	Sealed		= 0b________1,
}

public enum ResourceLinkChanges : byte
{
	None, Seal
}

public class ResourceLink : IBinarySerializable
{
	public EntityId			Destination { get; set; }
	public ResourceLinkFlag	Flags { get; set; }

	public bool				Affected;

	public ResourceLink Clone()
	{
		return new ResourceLink {Destination = Destination, Flags = Flags};
	}

	public void Read(BinaryReader reader)
	{
		Destination	= reader.Read<EntityId>();
		Flags		= (ResourceLinkFlag)reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Destination);
		writer.Write((byte)Flags);
	}
}

public class Resource// : IBinarySerializable
{
	public EntityId				Id { get; set; }
	public EntityId				Domain { get; set; }
	[JsonIgnore]public Ura		Address { get; set; }
	public ResourceFlags		Flags { get; set; }
	public ResourceData			Data { get; set; }
	public Time					Updated { get; set; }
	public ResourceLink[]		Outbounds { get; set; } = [];
	public EntityId[]			Inbounds { get; set; } = [];

	//public bool					New;
	//public bool					Affected;
	bool						OutboundsCloned;
	bool						InboundsCloned;

	public int					Length => (Flags.HasFlag(ResourceFlags.Data) ? (/*Data.Type.Control + Data.Type.Content.Length + */Data.Value.Length) : 0); /// Data.Type.Length + Data.ContentType.Length  - not fully precise

	public override string ToString()
	{
		return $"{Id}, {Address}, [{Flags}], Data={{{Data}}}, Outbounds={{{Outbounds.Length}}}, Inbounds={{{Inbounds.Length}}}";
	}

	public ResourceLink AffectOutbound(EntityId destination)
	{
		var i = Outbounds == null ? -1 : Array.FindIndex(Outbounds, i => i.Destination == destination);

		if(i == -1)
		{
			var l = new ResourceLink {Affected = true, Destination = destination};
			
			Outbounds = Outbounds == null ? [l] : Outbounds.Append(l).ToArray();
			OutboundsCloned = true;
			
			return l;
		} 
		else
		{
			if(!OutboundsCloned)
			{
				Outbounds = Outbounds.ToArray();
				OutboundsCloned = true;
			}

			if(!Outbounds[i].Affected)
			{	
				Outbounds[i] = Outbounds[i].Clone();
				Outbounds[i].Affected = true;
			}

			return Outbounds[i];
		}
	}

	public void AffectInbound(EntityId source)
	{
		var i = Inbounds == null ? -1 : Array.IndexOf(Inbounds, source);
		
		if(i != -1)
		{
			if(!InboundsCloned)
				Inbounds = Inbounds.ToArray();
		} 
		else
		{
			Inbounds = Inbounds == null ? [source] : Inbounds.Append(source).ToArray();
		}

		InboundsCloned = true;
	}

	public void RemoveOutbound(EntityId destination)
	{
		Outbounds = Outbounds.Where(i => i.Destination != destination).ToArray();
		OutboundsCloned = true;
	}

	public void RemoveInbound(EntityId destination)
	{
		Inbounds = Inbounds.Where(i => i != destination).ToArray();
		InboundsCloned = true;
	}
}

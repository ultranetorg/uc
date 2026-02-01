using System.Text.Json.Serialization;

namespace Uccs.Rdn;

[Flags]
public enum ResourceFlags : byte
{
	None			= 0, 
	Data			= 0b_______1,
	Dependable		= 0b______10, 
}

[Flags]
public enum ResourceChanges : byte
{
	None			= 0,
	SetData			= 0b________1,
	NullData		= 0b_______10,
	Dependable		= 0b______100,
	Recursive		= 0b1000_0000,
}

public enum ResourceLinkType : byte
{
	None,
	Hierarchy,
	Dependency,
	Extra,
	AntimalwareAnalysis,
}

public enum ResourceLinkChanges : byte
{
	None, Dependency
}

public class ResourceLink : IBinarySerializable
{
	public AutoId			Destination { get; set; }
	public ResourceLinkType	Type { get; set; }

	public bool				Affected;

	public ResourceLink Clone()
	{
		return new ResourceLink {Destination = Destination, Type = Type};
	}

	public void Read(BinaryReader reader)
	{
		Destination	= reader.Read<AutoId>();
		Type		= reader.Read<ResourceLinkType>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Destination);
		writer.Write(Type);
	}
}

public class Resource : ITableEntry
{
	public AutoId				Id { get; set; }
	public AutoId				Domain { get; set; }
	[JsonIgnore]public Ura		Address { get; set; }
	public ResourceFlags		Flags { get; set; }
	public ResourceData			Data { get; set; }
	public Time					Updated { get; set; }
	public ResourceLink[]		Outbounds { get; set; } = [];
	public AutoId[]				Inbounds { get; set; } = [];

	bool						OutboundsCloned;
	bool						InboundsCloned;
	public EntityId				Key => Id;
	public bool					Deleted { get; set; }
	RdnMcv						Mcv;

	public bool					IsLocked(RdnExecution execution) => Flags.HasFlag(ResourceFlags.Dependable) && Inbounds.Any(i => execution.Resources.Find(i).Outbounds.Any(j => j.Destination == Id && j.Type.HasFlag(ResourceLinkType.Dependency)));

	public int					DataLength => Flags.HasFlag(ResourceFlags.Data) ? Data.Value.Length : 0; /// Data.Type.Length + Data.ContentType.Length  - not fully precise

	public override string ToString()
	{
		return $"{Id}, {Address}, {Flags}, Data={{{Data}}}, Outbounds={{{Outbounds.Length}}}, Inbounds={{{Inbounds.Length}}}";
	}

	public Resource()
	{
	}

	public Resource(RdnMcv rdn)
	{
		Mcv = rdn;
	}

	public object Clone()
	{
		return	new Resource(Mcv)
				{
					Id = Id,
					Domain = Domain,
					Address = Address,
					Flags = Flags,
					Data = Data,
					Updated = Updated,
					Outbounds = Outbounds,
					Inbounds = Inbounds
				};
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write7BitEncodedInt(Domain.E);
		writer.WriteUtf8(Address.Resource);
		writer.Write(Updated);
		writer.Write(Flags);
		
		if(Flags.HasFlag(ResourceFlags.Data))
			writer.Write(Data);
	
		writer.Write(Outbounds);
		writer.Write(Inbounds);
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Domain	= new (Id.B, reader.Read7BitEncodedInt());
		Address = new Ura(null, reader.ReadUtf8());
		Updated	= reader.Read<Time>();
		Flags	= reader.Read<ResourceFlags>();

		if(Flags.HasFlag(ResourceFlags.Data))
			Data = reader.Read<ResourceData>();

		Outbounds	= reader.ReadArray<ResourceLink>();
		Inbounds	= reader.ReadArray<AutoId>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public ResourceLink AffectOutbound(AutoId destination)
	{
		var i = Outbounds == null ? -1 : Array.FindIndex(Outbounds, i => i.Destination == destination);

		if(i == -1)
		{
			var l = new ResourceLink {Affected = true, Destination = destination};
			
			Outbounds = [..Outbounds, l];
			OutboundsCloned = true;
			
			return l;
		} 
		else
		{
			if(!OutboundsCloned)
			{
				Outbounds = [..Outbounds];
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

	public void AffectInbound(AutoId source)
	{
		var i = Inbounds == null ? -1 : Array.IndexOf(Inbounds, source);
		
		if(i != -1)
		{
			if(!InboundsCloned)
				Inbounds = [..Inbounds];
		} 
		else
		{
			Inbounds = [..Inbounds, source];
		}

		InboundsCloned = true;
	}

	public void RemoveOutbound(AutoId destination)
	{
		var l = Outbounds.First(i => i.Destination == destination);
		Outbounds = Outbounds.Remove(l);
		OutboundsCloned = true;
	}

	public void RemoveInbound(AutoId destination)
	{
		Inbounds = Inbounds.Remove(destination);
		InboundsCloned = true;
	}
}

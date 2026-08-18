using System.Collections.Immutable;
using System.Text;

namespace Uccs.Rdn;

public class TextToMap : IBinarySerializable, ITableEntry<StringId>
{
	public StringId										Id { get; set; }
	public ImmutableSortedDictionary<AutoId, AutoId>	Entities { get; set; }

	public bool											Deleted { get; set; }
	Mcv													Mcv;

	public TextToMap()
	{
	}

	public TextToMap(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {nameof(Entities)}={{{Entities.Count}}}";
	}

	public object Clone()
	{
		var a = new TextToMap(Mcv)
				{
					Id			= Id,
					Entities	= Entities
				};
		
		return a;
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(Reader reader)
	{
		Entities = reader.ReadImmutableSortedDictionary(reader.Read<AutoId>,  reader.Read<AutoId>);
	}

	public void Write(Writer writer)
	{
		writer.Write(Entities, writer.Write, writer.Write);
	}
}


public class TextToEntity : IBinarySerializable, ITableEntry<StringId>
{
	public StringId					Id { get; set; }
	public AutoId					Entity { get; set; }

	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public TextToEntity()
	{
	}

	public TextToEntity(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, Entity={Entity}";
	}

	public object Clone()
	{
		var a = new TextToEntity(Mcv)
				{
					Id		= Id,
					Entity	= Entity
				};
		
		return a;
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(Reader reader)
	{
		Entity	= reader.Read<AutoId>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Entity);
	}
}


public class ResourceNameIndex : TextTable<TextToEntity> 
{
	public ResourceNameIndex(Mcv mcv) : base(mcv, RdnTable.ResourceName.ToString(), true)
	{
	}
	
	public override TextToEntity Create()
	{
		return new TextToEntity(Mcv);
	}

	public StringId GetId(string domain, string name)
	{
		var b = Encoding.UTF8.GetBytes($"{domain}/{name}");

		return new StringId(b);
	}
}

public class ResourceNameExecution : TextExecution<TextToEntity, ResourceNameIndex>
{
	public ResourceNameExecution(RdnExecution execution) : base(execution.Mcv.ResourceNames, execution)
	{
	}

	public override TextToEntity Affect(StringId id)
	{
		if(Affected.TryGetValue(id, out var a) && !a.Deleted)
			return a;
			
		if(Parent != null)
			a = Parent.Find(id);
		else if(!(Execution.Round as RdnRound).ResourceNames.Affected.TryGetValue(id, out a))
			a = Table.Find(id);

		if(a == null)
		{
			a = Table.Create();
			a.Id = id;
		
			return Affected[id] = a;
		} 
		else
		{
			return Affected[id] = a.Clone() as TextToEntity;
		}
	}

	public void Register(string domain, string name, AutoId resource)
	{
		var id = Table.GetId(domain, name);
		var w = Affect(id);
	
		w.Entity = resource;
	}

	public void Unregister(string domain, string name)
	{
		var id = Table.GetId(domain, name);
		var w = Affect(id);

		w.Deleted = true;
	}
}
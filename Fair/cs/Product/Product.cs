using System.Text;

namespace Uccs.Fair;

[Flags]
public enum ProductFlags : byte
{
	None, 
}

[Flags]
public enum ProductProperty : byte
{
	None,
}

public class ProductFieldVersionId  : IBinarySerializable
{
	public string		Name { get; set; }
	public int			Id { get; set; }

	public void Read(BinaryReader reader)
	{
		Name = reader.ReadString();
		Id = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write7BitEncodedInt(Id);
	}
}

public class ProductFieldVersion : IBinarySerializable
{
	public int			Id { get; set; }
	public byte[]		Value { get; set; }
	public int			Refs { get; set; }

	public void Read(BinaryReader reader)
	{
		Id = reader.Read7BitEncodedInt();
		Value = reader.ReadBytes();
		Refs = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.WriteBytes(Value);
		writer.Write7BitEncodedInt(Refs);
	}
}

public class ProductField : IBinarySerializable
{
	public string					Name { get; set; }
	public ProductFieldVersion[]	Versions  { get; set; }

	public int						Size =>  Versions.Sum(i => i.Value.Length);

	public const string				Title = "Title";
	public const string				Description = "Description";

	public const int				ValueLengthMaximum = 1024*1024;
	public const int				ValueNameMaximum = 256;
	
	public ProductField()
	{
	}

	public void Read(BinaryReader reader)
	{
		Name		= reader.ReadString();
		Versions	= reader.ReadArray<ProductFieldVersion>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(Versions);
	}
}

public class Product : IBinarySerializable
{
	public EntityId				Id { get; set; }
	public EntityId				AuthorId { get; set; }
	public ProductFlags			Flags { get; set; }
	public ProductField[]		Fields	{ get; set; }
	public Time					Updated { get; set; }
	public EntityId[]			Publications { get; set; }

	public int					Length => Mcv.EntityLength + Fields.Sum(i => i.Size); /// Data.Type.Length + Data.ContentType.Length  - not fully precise

	public override string ToString()
	{
		return $"{Id}, [{Flags}], Fields={Fields}";
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(AuthorId);
		writer.WriteEnum(Flags);
		writer.Write(Updated);
		writer.Write(Fields);
		writer.Write(Publications);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();
		AuthorId		= reader.Read<EntityId>();
		Flags			= reader.ReadEnum<ProductFlags>();
		Updated			= reader.Read<Time>();
		Fields			= reader.ReadArray<ProductField>();
		Publications	= reader.ReadArray<EntityId>();
	}
}

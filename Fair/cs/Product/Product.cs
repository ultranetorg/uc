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

public class ProductField : IBinarySerializable
{
	public string		Name { get; set; }
	public string		Value  { get; set; }

	public int			Size =>  Encoding.UTF8.GetByteCount(Value);

	public const string	Title = "Title";
	public const string	Description = "Description";
	
	public ProductField()
	{
	}

	public ProductField(string name, string value)
	{
		Name = name;
		Value = value;
	}

	public void Read(BinaryReader reader)
	{
		Name = reader.ReadString();
		Value = reader.ReadString();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(Value);
			
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
	public const int			DescriptionLengthMaximum = 1024*1024;

	public override string ToString()
	{
		return $"{Id}, [{Flags}], Fields={Fields}";
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(AuthorId);
		writer.Write((byte)Flags);
		writer.Write(Updated);
		writer.Write(Fields);
		writer.Write(Publications);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();
		AuthorId		= reader.Read<EntityId>();
		Flags			= (ProductFlags)reader.ReadByte();
		Updated			= reader.Read<Time>();
		Fields			= reader.ReadArray<ProductField>();
		Publications	= reader.ReadArray<EntityId>();
	}
}

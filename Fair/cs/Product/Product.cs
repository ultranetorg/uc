using System.Text;

namespace Uccs.Fair;

[Flags]
public enum ProductFlags : byte
{
	None		= 0, 
}

[Flags]
public enum ProductProperty : byte
{
	None			= 0,
	Description		= 1,
}

public class ProductField : IBinarySerializable
{
	public ProductProperty	Type;
	public object			Value;

	public ProductField()
	{
	}

	public ProductField(ProductProperty type, object value)
	{
		Type = type;
		Value = value;
	}

	public void Read(BinaryReader reader)
	{
		Type = (ProductProperty)reader.ReadByte();

		Value = Type switch
					 {
						ProductProperty.Description => reader.ReadUtf8(),
						_ => throw new RequirementException()
					 };
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)Type);

		switch(Type)
		{
			case ProductProperty.Description : writer.WriteUtf8(Value as string); break;
			default :
				throw new RequirementException();
		};
	}

	public static int GetSize(ProductProperty field, object value)
	{
		switch(field)
		{
			case ProductProperty.Description : return Encoding.UTF8.GetByteCount(value as string);
			default :
				throw new RequirementException();
		};
	}
}

public class Product// : IBinarySerializable
{
	public ProductId			Id { get; set; }
	public ProductFlags			Flags { get; set; }
	public ProductField[]		Fields	{ get; set; }
	public Time					Updated { get; set; }

	public short				Length => (short)(Mcv.EntityLength + Fields.Sum(i => ProductField.GetSize(i.Type, i.Value))); /// Data.Type.Length + Data.ContentType.Length  - not fully precise
	public const int			DescriptionLengthMaximum = 1024*1024;

	public override string ToString()
	{
		return $"{Id}, [{Flags}], Fields={Fields}";
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write((byte)Flags);
		writer.Write(Updated);
		writer.Write(Fields);
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<ProductId>();
		Flags	= (ProductFlags)reader.ReadByte();
		Updated	= reader.Read<Time>();
		Fields	= reader.ReadArray<ProductField>();
	}
}

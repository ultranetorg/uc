using System.Text;
using System.Text.Json.Serialization;

namespace Uccs.Fair;

[Flags]
public enum ProductFlags : byte
{
	None		= 0, 
}

[Flags]
public enum ProductField : byte
{
	None			= 0,
	Description		= 1,
}

public class CustomField : IBinarySerializable
{
	public ProductField		Type;
	public object			Value;

	public CustomField()
	{
	}

	public CustomField(ProductField type, object value)
	{
		Type = type;
		Value = value;
	}

	public void Read(BinaryReader reader)
	{
		Type = (ProductField)reader.ReadByte();

		Value = Type switch
					 {
						ProductField.Description => reader.ReadUtf8(),
						_ => throw new RequirementException()
					 };
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)Type);

		switch(Type)
		{
			case ProductField.Description : writer.WriteUtf8(Value as string); break;
			default :
				throw new RequirementException();
		};
	}
}

public class Product// : IBinarySerializable
{
	public ProductId			Id { get; set; }
	public ProductFlags			Flags { get; set; }
	public CustomField[]		Fields	{ get; set; }
	public Time					Updated { get; set; }

	public short				Length => (short)(Mcv.EntityLength + Fields.Sum(i => GetSize(i.Type, i.Value))); /// Data.Type.Length + Data.ContentType.Length  - not fully precise
	public const int			DescriptionLengthMaximum = 1024*1024;

	public override string ToString()
	{
		return $"{Id}, [{Flags}], Fields={Fields}";
	}

	public static int GetSize(ProductField field, object value)
	{
		switch(field)
		{
			case ProductField.Description : return Encoding.UTF8.GetByteCount(value as string);
			default :
				throw new RequirementException();
		};
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
		Fields	= reader.ReadArray<CustomField>();
	}
}

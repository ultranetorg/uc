using System.Text.Json.Serialization;

namespace Uccs.Fair;

[Flags]
public enum ProductFlags : byte
{
	None		= 0, 
}

[Flags]
public enum ProductChanges : byte
{
	None		= 0,
	SetData		= 1,
}

public class Product// : IBinarySerializable
{
	public ProductId			Id { get; set; }
	public ProductFlags			Flags { get; set; }
	public byte[]				Data { get; set; }
	public Time					Updated { get; set; }

	[JsonIgnore]
	public bool					New;
	public bool					Affected;

	public short				Length => (short)(Mcv.EntityLength + Data.Length); /// Data.Type.Length + Data.ContentType.Length  - not fully precise
	public const int			DataLengthMax = 1024*1024;

	public override string ToString()
	{
		return $"{Id}, [{Flags}], Data={{{Data}}}";
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write((byte)Flags);
		writer.Write(Updated);
		
		writer.WriteBytes(Data);
	}

	public void ReadMain(BinaryReader reader)
	{
		Id		= reader.Read<ProductId>();
		Flags	= (ProductFlags)reader.ReadByte();
		Updated	= reader.Read<Time>();

		Data = reader.ReadBytes();
	}
}

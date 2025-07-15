namespace Uccs.Fair;

public class File : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
    //public int				Refs { get; set; }
	public byte[]			Data { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public File()
	{
	}

	public File(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return new File(Mcv){Id		= Id,
							 //Refs	= Refs,
							 Data	= Data};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		//Refs	= reader.Read7BitEncodedInt();
		Data	= reader.ReadBytes();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		//writer.Write7BitEncodedInt(Refs);
		writer.WriteBytes(Data);
	}
}

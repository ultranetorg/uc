namespace Uccs.Fair;

public class Review : IBinarySerializable, ITableEntry<AutoId>
{
	public AutoId			Id { get; set; }
	public AutoId			Publication { get; set; }
    public AutoId			Creator { get; set; }
	public byte				Rating { get; set; }
    public string			Text { get; set; }
    public Time	    		Created { get; set; }

	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Review()
	{
	}

	public Review(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return new Review(Mcv) {Id			= Id,
								Publication = Publication,
								Creator		= Creator,
								Rating		= Rating,
								Text		= Text,
								Created		= Created};
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
		Id			= reader.Read<AutoId>();
		Publication	= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
		Rating		= reader.ReadByte();
		Text		= reader.ReadUtf8();
		Created		= reader.Read<Time>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Publication);
		writer.Write(Creator);
		writer.Write(Rating);
		writer.WriteUtf8(Text);
		writer.Write(Created);
	}
}

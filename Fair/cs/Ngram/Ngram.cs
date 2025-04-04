using System.Text;

namespace Uccs.Fair;

public class Ngram : IBinarySerializable, ITableEntry
{
	public RawId			Id { get; set; }
	public RawId[]			Ngrams { get; set; }
	public WordReference[]	References { get; set; }

	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;


	public Ngram()
	{
	}

	public Ngram(FairMcv mcv)
	{
		Mcv = mcv;
	}
	
	public static RawId	GetId(int n, string t, int start)
	{
		var b = Encoding.UTF8.GetBytes(t.ToLower(), start, n).Order();

		return new RawId(b.Count() > 2 ? b.ToArray() : (b.Count() == 1 ? [0,0, ..b] : [0, ..b]));
	}

	public Ngram Clone()
	{
		var a = new Ngram(Mcv)  {Id			= Id,
								 Ngrams		= Ngrams,
								 References	= References};

		return a;
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
		Id			= reader.Read<RawId>();
		Ngrams		= reader.ReadArray<RawId>();
		References	= reader.ReadArray<WordReference>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Ngrams);
		writer.Write(References);
	}
}

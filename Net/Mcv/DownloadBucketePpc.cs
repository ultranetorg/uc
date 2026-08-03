namespace Uccs.Net;

public class DownloadBucketPpc : McvPpc<DownloadBucketPpr>
{
	public byte		Table { get; set; }
	public byte[]	Hash { get; set; }
	public int		Bucket { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			if(Mcv.Tables[Table].IsIndex)
				throw new RequestException(RequestError.IncorrectRequest);

			var b = Mcv.Tables[Table].FindBucket(Bucket);

			if(b == null)
				throw new EntityException(EntityError.NotFound);

			if(!b.Hash.SequenceEqual(Hash))
				throw new EntityException(EntityError.HashMismatach);

			return new DownloadBucketPpr {Main = b.Export()};
		}
	}

	public override void Read(Reader reader)
	{
		Table = reader.ReadByte();
		Hash =  reader.ReadHash();
		Bucket = reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Table);
		writer.Write(Hash);
		writer.Write7BitEncodedInt(Bucket);
	}

}
	
public class DownloadBucketPpr : Result
{
	public byte[] Main { get; set; }

	public override void Read(Reader reader)
	{
		Main = reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Main);
	}
}

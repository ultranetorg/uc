namespace Uccs.Net;

public class DownloadBucketPpc : McvPpc<DownloadBucketPpr>, IBinarySerializable
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

	public void Read(Reader reader)
	{
		Table = reader.ReadByte();
		Hash =  reader.ReadHash();
		Bucket = reader.Read7BitEncodedInt();
	}

	public void Write(Writer writer)
	{
		writer.Write(Table);
		writer.Write(Hash);
		writer.Write7BitEncodedInt(Bucket);
	}

}
	
public class DownloadBucketPpr : Result, IBinarySerializable
{
	public byte[] Main { get; set; }

	public void Read(Reader reader)
	{
		Main = reader.ReadBytes();
	}

	public void Write(Writer writer)
	{
		writer.WriteBytes(Main);
	}
}

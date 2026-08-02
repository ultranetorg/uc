namespace Uccs.Net;

public class DownloadClusterPpc : McvPpc<DownloadClusterPpr>, IBinarySerializable
{
	public byte		Table { get; set; }
	public byte[]	Hash { get; set; }
	public short	Cluster { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			if(Mcv.Tables[Table].IsIndex)
				throw new RequestException(RequestError.IncorrectRequest);

			var c = Mcv.Tables[Table].FindCluster(Cluster);

			if(c == null)
				throw new EntityException(EntityError.NotFound);

			if(!c.Hash.SequenceEqual(Hash))
				throw new EntityException(EntityError.HashMismatach);

			return new DownloadClusterPpr {Main = c.Export()};
		}
	}

	public void Read(Reader reader)
	{
		Table = reader.ReadByte();
		Hash =  reader.ReadHash();
		Cluster = reader.ReadInt16();
	}

	public void Write(Writer writer)
	{
		writer.Write(Table);
		writer.Write(Hash);
		writer.Write(Cluster);
	}
}
	
public class DownloadClusterPpr : Result, IBinarySerializable
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

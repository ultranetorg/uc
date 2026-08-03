namespace Uccs.Rdn;

public class FileInfoPpc : RdnPpc<FileInfoPpr>
{
	public Urr		Release { get; set; }
	public string	File { get; set; }

	public override Result Execute()
	{
		lock(Node.ResourceHub.Lock)
		{
			if(Node.ResourceHub == null) 
				throw new NodeException(NodeError.NotSeed);
			
			var r = Node.ResourceHub.Find(Release);
			
			if(r == null || !r.IsReady(File)) 
				throw new EntityException(EntityError.NotFound);

			return new FileInfoPpr {Length = r.Find(File ?? "").Length};
		}
	}

	public override void Read(Reader reader)
	{
		Release = reader.ReadVirtual<Urr>();
		File	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteVirtual(Release);
		writer.WriteUtf8(File);
	}
}

public class FileInfoPpr : Result
{
	public long Length { get; set; }

	public override void Read(Reader reader)
	{
		Length = reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64(Length);
	}
}

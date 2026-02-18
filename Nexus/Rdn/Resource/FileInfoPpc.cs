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
}

public class FileInfoPpr : Result
{
	public long Length { get; set; }
}

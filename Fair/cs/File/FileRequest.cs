namespace Uccs.Fair;

public class FileRequest : FairPpc<FileResponse>
{
	public new AutoId	Id { get; set; }

	public FileRequest()
	{
	}

	public FileRequest(AutoId id)
	{
		Id = id;
	}

	public override PeerResponse Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Files.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new FileResponse {File = e};
		}
	}
}

public class FileResponse : PeerResponse
{
	public File	File {get; set;}
}

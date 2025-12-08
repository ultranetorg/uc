namespace Uccs.Fair;

public class FilePpc : FairPpc<FilePpr>
{
	public new AutoId	Id { get; set; }

	public FilePpc()
	{
	}

	public FilePpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		if(Id == null)
			throw new RequestException(RequestError.IncorrectRequest);

 		lock(Mcv.Lock)
		{	
			RequireGraph();

			var	e = Mcv.Files.Latest(Id);
			
			if(e == null)
				throw new EntityException(EntityError.NotFound);
			
			return new FilePpr {File = e};
		}
	}
}

public class FilePpr : Result
{
	public File	File {get; set;}
}


namespace Uccs.Fair;

public class FilePpc : FairPpc<FilePpr>
{
	public AutoId	Id { get; set; }

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

		RequireGraph();

		var	e = Mcv.Files.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new FilePpr {File = e};
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}
}

public class FilePpr : Result
{
	public File	File {get; set;}

	public override void Read(Reader reader)
	{
		File = reader.Read<File>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(File);
	}
}

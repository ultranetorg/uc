namespace Uccs.Fair;

public class ReviewPpc : FairPpc<ReviewPpr>
{
	public AutoId	Id { get; set; }

	public ReviewPpc()
	{
	}

	public ReviewPpc(AutoId id)
	{
		Id = id;
	}

	public override Result Execute()
	{
		RequireGraph();

		var	e = Mcv.Reviews.Latest(Id);
			
		if(e == null)
			throw new EntityException(EntityError.NotFound);
			
		return new ReviewPpr {Review = e};
	}
	
	public override void Write(Writer writer)
	{
		writer.Write(Id);
	}

	public override void Read(Reader reader)
	{
		Id = reader.Read<AutoId>();
	}
}

public class ReviewPpr : Result
{
	public Review	Review {get; set;}

	public override void Read(Reader reader)
	{
		Review = reader.Read<Review>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Review);
	}
}

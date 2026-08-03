namespace Uccs.Rdn;

public class QueryResourcePpc : RdnPpc<QueryResourcePpr>
{
	public AutoId		Domain { get; set; }
	public string		Query { get; set; }

	public override Result Execute()
	{
		return new QueryResourcePpr {Resources = Mcv.SearchResources(Domain, Query).ToArray()};
	}

	public override void Read(Reader reader)
	{
		Domain = reader.Read<AutoId>();
		Query	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Domain);
		writer.WriteUtf8(Query);
	}
}
	
public class QueryResourcePpr : Result
{
	public Resource[] Resources { get; set; }

	public override void Read(Reader reader)
	{
		Resources = reader.ReadArray<Resource>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Resources);
	}
}

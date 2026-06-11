namespace Uccs.Net;

public class DoIcca : IccpArgumentation
{
	public string	Query { get; set; }

	public override void Read(Reader reader)
	{
		Query = reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Query);
	}
}

public class DoIccr : IccpResult
{
	public byte[]			Response { get; set; }

	public override void	Read(Reader reader) => Response = reader.ReadBytes();
	public override void	Write(Writer writer) => writer.WriteBytes(Response);
}

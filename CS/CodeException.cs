namespace Uccs;

public abstract class CodeException : Exception, ITypeCode, IBinarySerializable 
{
	public abstract int		Code { get; set; }
	public string 			Details { get; set; }

	public override string Message  => Details;

	static CodeException()
	{
	}

	public CodeException()
	{
	}

	public CodeException(string message)
	{
		Details = message;
	}

	public virtual void Read(Reader reader)
	{
		Code = reader.Read7BitEncodedInt();
		Details = reader.ReadUtf8();
	}

	public virtual void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Code);
		writer.WriteUtf8(Details);
	}
}

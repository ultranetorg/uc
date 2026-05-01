namespace Uccs;

public abstract class CodeException : Exception, ITypeCode, IBinarySerializable 
{
	public abstract int		ErrorCode { get; set; }

	static CodeException()
	{
	}

	public CodeException()
	{
	}

	public CodeException(string message) : base(message)
	{
	}

	public void Read(Reader reader)
	{
		ErrorCode = reader.Read7BitEncodedInt();
	}

	public void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(ErrorCode);
	}
}

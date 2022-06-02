namespace UC
{
	public interface IXonReader
	{
		XonToken				Read(XonValueSerializator serializator);
		XonToken				Current{ get; }
		XonToken				ReadNext();
		string					ParseName();
		object					ParseMeta();
		object					ParseValue();
	}
}
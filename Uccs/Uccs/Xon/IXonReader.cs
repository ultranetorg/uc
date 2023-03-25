namespace Uccs
{
	public interface IXonReader
	{
		XonToken				Read(IXonValueSerializator serializator);
		XonToken				Current{ get; }
		XonToken				ReadNext();
		string					ParseName();
		object					ParseMeta();
		object					ParseValue();
	}
}
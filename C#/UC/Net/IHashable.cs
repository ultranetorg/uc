using System.IO;

namespace UC.Net
{
	public interface IHashable
	{
		 void HashWrite(BinaryWriter w);
	}
}
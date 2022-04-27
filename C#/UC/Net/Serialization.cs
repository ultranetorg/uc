using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public interface IBinarySerializable
	{
		void Read(BinaryReader r);
		void Write(BinaryWriter w);
	}

	public interface ITypedBinarySerializable : IBinarySerializable
	{
		byte BinaryType { get; }
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs
{
	public interface IBinarySerializable
	{
		void Write(BinaryWriter writer);
		void Read(BinaryReader reader);

	}

	public interface ITypeCode
	{
		byte TypeCode { get; }
	}
}

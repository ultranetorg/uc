using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Uccs.Net
{
	public interface ITableEntry<K>
	{
		K		Key { get; }
		byte[]	GetClusterKey(int n);

		void	ReadMain(BinaryReader r);
		void	WriteMain(BinaryWriter r);

		void	ReadMore(BinaryReader r);
		void	WriteMore(BinaryWriter r);


	}
}

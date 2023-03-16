using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UC.Net
{
	public abstract class TableEntry<K> : IBinarySerializable
	{
		public const int		ClusterKeyLength = 2;
		public DateTime			LastAccessed;
		public abstract	K		Key { get; }
		public abstract	byte[]	ClusterKey { get; }

		public abstract			void Read(BinaryReader r);
		public abstract			void Write(BinaryWriter r);

		public abstract			void ReadMore(BinaryReader r);
		public abstract			void WriteMore(BinaryWriter r);
	}
}

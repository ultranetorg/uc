using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UC.Net
{
	public abstract class Entry<K>
	{
		public DateTime		LastAccessed;
		public abstract		K Key { get; }
		public abstract		void Read(BinaryReader r);
		public abstract		void Write(BinaryWriter r);
	}
}

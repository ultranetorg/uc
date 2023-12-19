using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Uccs.Net
{
	public interface ITableEntry<K>
	{
		EntityId	Id { get; set; }
		K			Key { get; }

		void		ReadMain(BinaryReader r);
		void		WriteMain(BinaryWriter r);

		void		ReadMore(BinaryReader r);
		void		WriteMore(BinaryWriter r);
	}
}

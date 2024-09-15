namespace Uccs.Net
{
	public interface ITableEntryBase
	{
		EntityId	Id { get; set; }
		//bool		New { get; set; }

		void		ReadMain(BinaryReader r);
		void		WriteMain(BinaryWriter r);

		void		ReadMore(BinaryReader r);
		void		WriteMore(BinaryWriter r);
	}

	public interface ITableEntry<K> : ITableEntryBase
	{
		K			Key { get; }
	}
}

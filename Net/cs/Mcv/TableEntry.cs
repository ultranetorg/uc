namespace Uccs.Net
{
	public interface ITableEntryBase
	{
		EntityId	Id { get; set; }
		//bool		New { get; set; }

		void		Cleanup(Round lastInCommit);

		void		ReadMain(BinaryReader r);
		void		WriteMain(BinaryWriter r);

		void		ReadMore(BinaryReader r);
		void		WriteMore(BinaryWriter r);
	}

	public interface ITableEntry : ITableEntryBase
	{
	}
}

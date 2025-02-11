namespace Uccs.Net;

public interface ITableEntry
{
	EntityId	Id { get; }
	//bool		New { get; set; }
	bool		Deleted { get; }

	void		Cleanup(Round lastInCommit);

	void		ReadMain(BinaryReader r);
	void		WriteMain(BinaryWriter r);

	void		ReadMore(BinaryReader r);
	void		WriteMore(BinaryWriter r);
}

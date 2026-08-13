using System.Text;
using RocksDbSharp;

namespace Uccs.Net;

public abstract class UserTable : Table<AutoId, User>
{
	public abstract User Find(string nickname);

	public UserTable(Mcv chain) : base(chain, McvTable.User.ToString())
	{
	}

	public override User Create()
	{
		return new User(Mcv);
	}

	public User Latest(string name)
	{
		if(Mcv.LastConfirmedRound.AffectedUsers.Values.FirstOrDefault(i => i.Name == name) is User e && !e.Deleted)
			return e;

		return Find(name);
	}
}

using System.Text;

namespace Uccs.Net;

public class UserTable : Table<AutoId, User>
{
	public override string Name => McvTable.User.ToString();

	public int	KeyToBucket(string name) => EntityId.BytesToBucket(Encoding.UTF8.GetBytes(name.PadRight(3, '\0'), 0, 3));

	public UserTable(Mcv chain) : base(chain)
	{
	}

	public override User Create()
	{
		return new User(Mcv);
	}

	public User FindEntry(string nickname)
	{
		var bid = KeyToBucket(nickname);

		return FindBucket(bid)?.Entries.FirstOrDefault(i => i.Name == nickname);
	}

	public User Find(string nickname, int ridmax)
	{
		foreach(var r in Mcv.Tail.Where(i => i.Id <= ridmax))
			if(r.AffectedAccounts.Values.FirstOrDefault(i => i.Name == nickname) is User e && !e.Deleted)
				return e;

		return FindEntry(nickname);
	}

	public User Latest(string nickname)
	{
		return Find(nickname, Mcv.LastConfirmedRound.Id);
	}
}

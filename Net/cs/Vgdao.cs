namespace Uccs.Net.Vgo
{
	public enum TransactionType
	{
		None, MembershipRequest, BaseChange,
	}

	public class Vote
	{
		public EntityId		Member;
		public bool			Approved;
		public byte[]		BaseHash;
	}

	public class Transaction
	{
		public TransactionType	Type;
		public EntityId			Sender;
		public byte[]			Data;

		public List<Vote>		Votes;
	}

	public class Member
	{
		public EntityId		Account;
		public int			Age;
	}

	public class Organization
	{
		public List<EntityId>		Members;
		public byte[]				BaseHash;
		public List<Transaction>	Tail;

		public int					Age(EntityId member) => Members.BinarySearch(member);

		public const int			CommitLength = 100;
		public int					MinVotes => Math.Min(21, Members.Count);
		public int					MaxVotes => 100;

		public void Commit()
		{
			foreach(var i in Tail.AsEnumerable().Reverse())
			{
				if(i.Votes.Count >= MinVotes && i.Votes.OrderBy(i => Age(i.Member)).Take(MinVotes).Count(v => v.Approved) > MinVotes/2)
				{
					if(i.Type == TransactionType.MembershipRequest)
					{
						Members.Add(i.Sender);
					}
					
					BaseHash = i.Votes.First(i => i.Approved).BaseHash;
				}
			}
		}

		public bool Vote(Transaction transaction, Vote vote)
		{
			transaction.Votes ??= [];

			if(Members.Contains(vote.Member) && transaction.Votes.Count < MaxVotes)
			{
				transaction.Votes.Add(vote);
				
				return true;
			}
			else
				return false;
		}
	}
}

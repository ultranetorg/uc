namespace Uccs.Net;

public class Execution
{
	public Dictionary<AutoId, Account>			AffectedAccounts = new();
	public Dictionary<AutoId, Generator>		AffectedCandidates = new();
	public Dictionary<int, int>[]				NextEids;
	public AutoId								LastCreatedId;
	public long[]								Spacetimes;
	public long[]								Bandwidths;

	public List<Generator>						Candidates;

	public Time									Time;
	public McvNet								Net;
	public Mcv									Mcv;
	public Round								Round;
	public Transaction							Transaction;

	public Execution(Mcv mcv, Round round, Transaction transaction)
	{
		Net = mcv.Net;
		Mcv = mcv;
		Round = round;
		Transaction = transaction;
		Time = round.ConsensusTime;

		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();

		Candidates = round.Candidates;
		Spacetimes = round.Spacetimes;
		Bandwidths = round.Bandwidths;
	}

	public virtual ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Accounts.Id == table)	
			return FindAccount(id as AutoId) != null ? AffectAccount(id as AutoId) : null;

		return null;
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Accounts)	return AffectedAccounts;

		throw new IntegrityException();
	}

	public Dictionary<K, E> AffectedByTable<K, E>(TableBase table)
	{
		return AffectedByTable(table) as Dictionary<K, E>;
	}

	public int GetNextEid(TableBase table,  int b)
	{
		int e = 0;

		NextEids[table.Id].TryGetValue(b, out e);

		if(e == 0)
		{
			foreach(var r in Mcv.Tail.Where(i => i.Id <= Round.Id))
			{	
				var eids = r.NextEids[table.Id];

				if(eids != null && eids.TryGetValue(b, out e))
					break;
			}
		}
			
		if(e == 0)
			e = table.FindBucket(b)?.NextEid ?? 0;

		NextEids[table.Id][b] = e + 1;

		return e;
	}
	
	public void TransferEnergyIfNeeded(IEnergyHolder a)
	{
		if(a.EnergyThisPeriod != Time.Days/Net.ECLifetime.Days)
		{
			if(a.EnergyThisPeriod + 1 == Time.Days/Net.ECLifetime.Days)
				a.Energy = a.EnergyNext;
	
			a.EnergyNext = 0;
			a.EnergyThisPeriod	= (byte)(Time.Days/Net.ECLifetime.Days);
		}
	}

	public virtual Account AffectSigner()
	{
 		if(Transaction.Signer == Net.God)
 			return new Account {Address = Net.God};

		var s = Mcv.Accounts.Find(Transaction.Signer, Round.Id);

		if(s == null)
		{
			foreach(var o in Transaction.Operations)
				o.Error = Operation.NotFound;
					
			return null;
		}
	
		if(Transaction.Nid != s.LastTransactionNid + 1)
		{
			foreach(var o in Transaction.Operations)
				o.Error = Operation.NotSequential;
					
			return null;
		}

		return AffectAccount(s.Id);
	}

	public Account FindAccount(AutoId id)
	{
		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		return Mcv.Accounts.Find(id, Round.Id);
	}

	public Account FindAccount(AccountAddress address)
	{
		if(AffectedAccounts.Values.FirstOrDefault(i => i.Address == address) is Account a)
			return a;

		return Mcv.Accounts.Find(address, Round.Id);
	}

	public virtual Account CreateAccount(AccountAddress address)
	{
		var b = Mcv.Accounts.KeyToBucket(address);
			
		int e = GetNextEid(Mcv.Accounts, b);

		var a = Mcv.Accounts.Create();

		a.Id		= LastCreatedId = new AutoId(b, e);
		a.Address	= address;

		AffectedAccounts[a.Id] = a;

		return a;
	}

	public Account AffectAccount(AutoId id)
	{
		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		a = Mcv.Accounts.Find(id, Round.Id)?.Clone() as Account;

		AffectedAccounts[a.Id] = a;

		TransferEnergyIfNeeded(a);

		return a;
	}

	public Generator AffectCandidate(AutoId id)
	{
		if(AffectedCandidates.TryGetValue(id, out Generator a))
			return a;

		if(Candidates == Round.Candidates)
			Candidates = Round.Candidates.ToList();

		var c = Candidates.Find(i => i.Id == id);

		if(c == null)
		{
			c = AffectedCandidates[id] = Mcv.CreateGenerator();

			Candidates.Add(c);
		
			if(Candidates.Count > Mcv.Net.CandidatesMaximum)
				Candidates.RemoveAt(0);
		}
		else
		{
			c = AffectedCandidates[id] = c.Clone();
		}

		return c;
	}

}

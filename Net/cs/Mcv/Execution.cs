namespace Uccs.Net;

public class Execution
{
	public Dictionary<EntityId, AccountEntry>	AffectedAccounts = new();
	public Dictionary<EntityId, Generator>		AffectedCandidates = new();
	public Dictionary<int, int>[]				NextEids;
	public EntityId								LastCreatedId;
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
			return FindAccount(id) != null ? AffectAccount(id) : null;

		return null;
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

	public virtual AccountEntry AffectSigner()
	{
 		if(Transaction.Signer == Net.God)
 			return new AccountEntry {Address = Net.God};

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

	public AccountEntry FindAccount(EntityId id)
	{
		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		return Mcv.Accounts.Find(id, Round.Id);
	}

	public AccountEntry FindAccount(AccountAddress address)
	{
		if(AffectedAccounts.Values.FirstOrDefault(i => i.Address == address) is AccountEntry a)
			return a;

		return Mcv.Accounts.Find(address, Round.Id);
	}

	public virtual AccountEntry CreateAccount(AccountAddress address)
	{
		var b = Mcv.Accounts.KeyToBid(address);
			
		int e = GetNextEid(Mcv.Accounts, b);

		var a = Mcv.Accounts.Create();

		a.Id		= LastCreatedId = new EntityId(b, e);
		a.Address	= address;
		a.New		= true;

		AffectedAccounts[a.Id] = a;

		return a;
	}

	public AccountEntry AffectAccount(EntityId id)
	{
		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		a = Mcv.Accounts.Find(id, Round.Id)?.Clone();	

		AffectedAccounts[a.Id] = a;

		TransferEnergyIfNeeded(a);

		return a;
	}

	public Generator AffectCandidate(EntityId id)
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

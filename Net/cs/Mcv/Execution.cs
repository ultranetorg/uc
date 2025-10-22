namespace Uccs.Net;

public class Execution : ITableExecution
{
	public Dictionary<MetaId, MetaEntity>		AffectedMetas = new();
	public Dictionary<AutoId, Account>			AffectedAccounts = new();
	public Dictionary<AutoId, Generator>		AffectedCandidates = new();
	public Dictionary<int, int>[]				NextEids;
	public long[]								Spacetimes;
	public long[]								Bandwidths;

	public List<Generator>						Candidates;

	public Time									Time;
	public McvNet								Net;
	public Mcv									Mcv;
	public Round								Round;
	public Transaction							Transaction;
	//protected Account							AffectedSigner;

	public AutoId								LastCreatedId { get; set; }

	//public IEnergyHolder						EnergyFeePayer;
	public HashSet<IEnergyHolder>				EnergySpenders;
	public HashSet<ISpacetimeHolder>			SpacetimeSpenders;
	public long									ECEnergyCost;

	public Execution							Parent;

	public Execution(Mcv mcv, Round round, Transaction transaction)
	{
		Net = mcv.Net;
		Mcv = mcv;
		Round = round;
		Transaction = transaction;
		//Signer = signer;
		Time = round.ConsensusTime;

		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();

		Candidates = round.Candidates;
		Spacetimes = round.Spacetimes;
		Bandwidths = round.Bandwidths;
	}

	public virtual ITableExecution FindExecution(byte table)
	{
		if(Mcv.Accounts.Id == table) return this;

		return null;
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

	public MetaEntity AffectMeta(MetaId id)
	{
		if(AffectedMetas.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			Parent.AffectedMetas.TryGetValue(id, out a);
		else
			a = Mcv.Metas.Find(id, Round.Id);
		
		if(a == null)
		{
			a = Mcv.Metas.Create();
			a.Id = id;
		}
		else
			a = a.Clone() as MetaEntity;

		AffectedMetas[a.Id] = a;

		return a;
	}

	public void IncrementCount(int type)
	{
		var m = AffectMeta(new MetaId(type, []));
		m.Value = m.Value == null ? [1, 0, 0, 0] : BitConverter.GetBytes(BitConverter.ToInt32(m.Value) + 1);
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
			e = table.FindBucket(b)?.NextE ?? 0;

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

	public void PayCycleEnergy(IEnergyHolder spender)
	{
		if(spender.BandwidthExpiration >= Time.Days)
		{
			if(spender.BandwidthTodayTime < Time.Days) /// switch to this day
			{	
				spender.BandwidthTodayTime		= Time.Days;
				spender.BandwidthTodayAvailable	= spender.Bandwidth;
			}

			spender.BandwidthTodayAvailable -= ECEnergyCost;
		}
		else
		{
			spender.Energy -= ECEnergyCost;

			Transaction.EnergyConsumed += ECEnergyCost;
		}

		EnergySpenders.Add(spender);
	}

	public void AllocateForever(ISpacetimeHolder payer, int length)
	{
		payer.Spacetime -= ToBD(length, Mcv.Forever);
		SpacetimeSpenders.Add(payer);
	}

	public void FreeForever(ISpacetimeHolder payer, int length)
	{
		payer.Spacetime += ToBD(length, Mcv.Forever);
	}

	public void FreeEntity()
	{
		Spacetimes[0] += ToBD(Transaction.Net.EntityLength, Mcv.Forever); /// to be distributed between members
	}

	public static long ToBD(long length, short time)
	{
		return time * length;
	}

	public static long ToBD(long length, Time time)
	{
		return time.Days * length;
	}

	public void Allocate(ISpacetimeHolder payer, ISpaceConsumer consumer, int space)
	{
		if(space == 0)
			return;

		consumer.Space += space;

		var n = consumer.Expiration - Time.Days;
	
		payer.Spacetime -= ToBD(space, (short)n);

		for(int i = 0; i < n; i++)
			Spacetimes[i] += space;

		SpacetimeSpenders.Add(payer);
	}

	public void Prolong(ISpacetimeHolder payer, ISpaceConsumer consumer, Time duration)
	{	
		var start = consumer.Expiration < Time.Days ? Time.Days : consumer.Expiration;

		consumer.Expiration = (short)(start + duration.Days);

		if(consumer.Space > 0)
		{
			payer.Spacetime -= ToBD(consumer.Space, duration);
			SpacetimeSpenders.Add(payer);
		}

		var n = start + duration.Days - Time.Days;

		if(n > Spacetimes.Length)
			Spacetimes = [..Spacetimes, ..new long[n - Spacetimes.Length]];

		for(int i = 0; i < duration.Days; i++)
			Spacetimes[start - Time.Days + i] += consumer.Space;

	}

	public void Free(ISpacetimeHolder beneficiary, ISpaceConsumer consumer, long space)
	{
		if(space == 0)
			return;

		consumer.Space -= space;

		if(consumer.Space < 0)
			throw new IntegrityException();

		var d = consumer.Expiration - Time.Days;
		
		if(d > 0)
		{
			beneficiary.Spacetime += ToBD(space, (short)(d - 1));
	
			for(int i = 1; i < d; i++)
				Spacetimes[i] -= space;
		}
	}

	public virtual Account AffectSigner()
	{
		if(Transaction.Sponsored)
		{
			if(AffectedAccounts.FirstOrDefault(i => i.Value.Address == Transaction.Signer).Value is Account a)
				return a;
		
			if(Parent != null)
				a = Parent.FindAccount(Transaction.Signer);
			else
				a = Mcv.Accounts.Find(Transaction.Signer, Round.Id);	

			if(a == null)
				a = CreateAccount(Transaction.Signer);
			else
				a = AffectAccount(a.Id);

			return a;
		}
		else
		{
 			if(Transaction.Signer == Mcv.God)
 				return new Account {Address = Mcv.God};

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
	}

	public Account FindAccount(AutoId id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId : id;

		if(id == null)
			return null;

		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			return Parent.FindAccount(id);

		return Mcv.Accounts.Find(id, Round.Id);
	}

	public Account FindAccount(AccountAddress address)
	{
		if(AffectedAccounts.Values.FirstOrDefault(i => i.Address == address) is Account a)
			return a;

		if(Parent != null)
			return Parent.FindAccount(address);

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

		IncrementCount((int)MetaEntityType.AccountCount);

		return a;
	}

	public Account AffectAccount(AutoId id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId : id;

		if(AffectedAccounts.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			a = Parent.FindAccount(id);
		else
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

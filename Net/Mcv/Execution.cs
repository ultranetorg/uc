namespace Uccs.Net;

public class Execution : ITableExecution
{
	public Dictionary<MetaId, MetaEntity>		AffectedMetas = new();
	public Dictionary<AutoId, User>				AffectedUsers = new();
	public Dictionary<AutoId, Generator>		AffectedCandidates = new();
	
	Dictionary<int, int>[]						_NextEids;
	long[]										_Spaces;
	long[]										_Bandwidths;
	List<Generator>								_Candidates;
	
	public Dictionary<int, int>[]				NextEids => _NextEids ??= [..Mcv.Tables.Select(i => new Dictionary<int, int>())];
	public long[]								Spaces  { get => _Spaces ?? Round.Spacetimes; set => _Spaces = value; }
	public long[]								Bandwidths  { get => _Bandwidths ?? Round.Bandwidths; set => _Bandwidths = value; }
	public List<Generator>						Candidates  { get => _Candidates ?? Round.Candidates; set => _Candidates = value; }

	public Time									Time => Round.ConsensusTime;
	public McvNet								Net;
	public Mcv									Mcv;
	public Round								Round;
	public Transaction							Transaction;

	public AutoId								LastCreatedId { get; set; }

	public HashSet<IEnergyHolder>				EnergySpenders;
	public HashSet<ISpacetimeHolder>			SpacetimeSpenders;
	public long									EnergyCost;

	public Execution							Parent;

	public Execution(Mcv mcv, Round round, Transaction transaction)
	{
		Net = mcv.Net;
		Mcv = mcv;
		Round = round;
		Transaction = transaction;
	}

	public virtual ITableExecution FindExecution(byte table)
	{
		if(Mcv.Users.Id == table) return this;

		return null;
	}

	public virtual ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Users.Id == table)	
			return FindUser(id as AutoId) != null ? AffectUser(id as AutoId) : null;

		return null;
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Users)	return AffectedUsers;

		throw new IntegrityException();
	}

	public void AffectBandwidths()
	{
		_Bandwidths ??= [..Round.Bandwidths];
	}

	public void AffectSpaces()
	{
		_Spaces ??= [..Round.Spacetimes];
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
			if(a.EnergyThisPeriod + 1 == Time.Days/Net.ECLifetime.Days) /// if this is next period only
				a.Energy = a.EnergyNext;
	
			a.EnergyNext = 0;
			a.EnergyThisPeriod	= (byte)(Time.Days/Net.ECLifetime.Days);
		}
	}

	public void PayCycleEnergy(IEnergyHolder spender)
	{
		if(spender.BandwidthPeriod < Time.Hours) /// switch to this day
		{	
			if(spender.BandwidthExpiration < Time.Hours) /// bandwidth expired
				spender.Bandwidth = 0;

			spender.BandwidthPeriod			= Time.Hours;
			spender.BandwidthPeriodBalance	= spender.Bandwidth;
		}

		spender.BandwidthPeriodBalance -= (int)EnergyCost;

		/// var d = spender.BandwidthTodayBalance - EnergyCost;
		/// 
		/// if(d < 0)
		/// {
		/// 	d = -d;
		/// 	spender.Energy -= Math.Min(d, EnergyCost);
		/// 	EnergySpenders.Add(spender);
		/// }
		/// 
		Transaction.EnergyConsumed += EnergyCost;
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
		AffectSpaces();

		Spaces[Time.Days] += ToBD(Transaction.Net.EntityLength, Mcv.Forever); /// to be distributed between members
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

		AffectSpaces();

		for(int i = 0; i < n; i++)
			Spaces[i] += space;

		SpacetimeSpenders.Add(payer);
	}

	public void Prolong(ISpacetimeHolder payer, ISpaceConsumer consumer, Time duration)
	{	
		var now = Time.Days;
		var start = now >= consumer.Expiration ? now : consumer.Expiration;

		consumer.Expiration = (short)(start + duration.Days);

		if(consumer.Space > 0)
		{
			payer.Spacetime -= ToBD(consumer.Space, duration);
			SpacetimeSpenders.Add(payer);
		}

		var exp = start + duration.Days;

		if(exp - now >	Spaces.Length)
			Spaces = [..Spaces, ..new long[Spaces.Length + exp - now]];
		else
			AffectSpaces(); /// needed below

		for(int i = start - now; i < exp - now; i++)
			Spaces[i] += consumer.Space;

	}

	public void Free(ISpacetimeHolder beneficiary, ISpaceConsumer consumer, long space)
	{
		if(space == 0)
			return;

		var now = Time.Days;

		consumer.Space -= space;

		if(consumer.Space < 0)
			throw new IntegrityException();

		var d = consumer.Expiration - now;
		
		if(d > 0)
		{
			beneficiary.Spacetime += ToBD(space, (short)(d - 1));
	
			AffectSpaces();
			
			for(int i = 0; i < consumer.Expiration - now; i++)
				Spaces[i] -= space;
		}
	}

	public virtual User AffectSigner()
	{
 		if(Round.Id == 0)
 			return new User {Name = Mcv.GodName, Owner = Mcv.God.Address};

		var u = Transaction.User;

		if(AffectedUsers.FirstOrDefault(i => i.Value.Name == u).Value is User s)
			return s;
		
		if(Parent != null)
			s = Parent.FindUser(u);
		else
			s = Mcv.Users.Find(u, Round.Id);

		if(s == null)
		{	
			if(!Operation.IsFreeNameValid(u))
			{
				Transaction.Error = Operation.InvalidName;
				return null;
			}
		
			s = CreateUser(u, Transaction.Signer);
		}
		else
		{	
			if(Transaction.Signature != null && Transaction.Signer != s.Owner)
			{
				Transaction.Error = Operation.Denied;
				return null;
			}

			if(Transaction.Nonce != s.LastNonce + 1)
			{
				Transaction.Error = Operation.NotSequential;
				return null;
			}

			s = AffectUser(s.Id);
		}

		return s;
	}

	public User FindUser(AutoId id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId : id;

		if(id == null)
			return null;

		if(AffectedUsers.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			return Parent.FindUser(id);

		return Mcv.Users.Find(id, Round.Id);
	}

	public User FindUser(string name)
	{
		if(AffectedUsers.Values.FirstOrDefault(i => i.Name == name) is User a)
			return a;

		if(Parent != null)
			return Parent.FindUser(name);

		return Mcv.Users.Find(name, Round.Id);
	}

	public virtual User CreateUser(string name, AccountAddress owner)
	{
		var b = Mcv.Users.KeyToBucket(name);
			
		int e = GetNextEid(Mcv.Users, b);

		var a = Mcv.Users.Create();

		a.Id	= LastCreatedId = new AutoId(b, e);
		a.Name	= name;
		a.Owner	= owner;

		AffectedUsers[a.Id] = a;

		IncrementCount((int)MetaEntityType.AccountCount);

		return a;
	}

	public User AffectUser(AutoId id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId : id;

		if(AffectedUsers.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			a = Parent.FindUser(id);
		else
			a = Mcv.Users.Find(id, Round.Id)?.Clone() as User;

		AffectedUsers[a.Id] = a;

		TransferEnergyIfNeeded(a);

		return a;
	}

	public Generator AffectCandidate(AutoId id)
	{
		if(AffectedCandidates.TryGetValue(id, out Generator a))
			return a;

		_Candidates ??= [..Round.Candidates];

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

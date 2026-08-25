using System.Runtime.CompilerServices;

namespace Uccs.Net;

public class Execution : ITableExecution
{
	public Dictionary<MetaId, MetaEntity>		AffectedMetas = new();
	public Dictionary<AutoId, User>				AffectedUsers = new();
	//public Dictionary<AutoId, Member>			AffectedCandidates = new();
	
	public FrientExecution						Friends;

	Dictionary<int, int>[]						_NextEids;
	long[]										AffectedSpaces;
	long[]										AffectedBandwidths;
	List<OutwardTransaction>					AffectedOutwardTransactions;
	OrderedDictionary<IccpTransaction, string>	AffectedIccTransaction;
	List<Member>								_Candidates;
	
	public Dictionary<int, int>[]				NextEids => _NextEids ??= [..Mcv.Tables.Select(i => new Dictionary<int, int>())];
	public long[]								Spaces  { get => AffectedSpaces ?? Round.Spacetimes; set => AffectedSpaces = value; }
	public long[]								Bandwidths { get => AffectedBandwidths ?? Round.Bandwidths; set => AffectedBandwidths = value; }
	public List<Member>							Candidates { get => _Candidates ?? Round.Candidates; }
	public List<OutwardTransaction>				OutwardTransactions { get => AffectedOutwardTransactions ?? Round.OutwardTransactions; set => AffectedOutwardTransactions = value; }
	public OrderedDictionary<IccpTransaction, string>	IccTransactions { get => AffectedIccTransaction ?? Round.IccTransactions; set => AffectedIccTransaction = value; }

	public Time									Time => Round.ConsensusTime;
	public McvNet								Net;
	public Mcv									Mcv;
	public Round								Round;
	public Transaction							Transaction;

	public AutoId								LastCreatedId { get; set; }

	public HashSet<IEnergyHolder>				EnergySpenders;
	public HashSet<ISpacetimeHolder>			SpacetimeSpenders;
	public long									OperationCost;

	public Execution							Parent;

	public Execution(Mcv mcv, Round round, Transaction transaction)
	{
		Net = mcv.Net;
		Mcv = mcv;
		Round = round;
		Transaction = transaction;
		Friends = new(this);
	}

	public void AffectBandwidths()
	{
		AffectedBandwidths ??= [..Round.Bandwidths];
	}

	public void AffectSpaces()
	{
		AffectedSpaces ??= [..Round.Spacetimes];
	}

	public void AffectOutwards()
	{
		AffectedOutwardTransactions ??= [..Round.OutwardTransactions];
	}

	public void AffectIccTransactions()
	{
		AffectedIccTransaction ??= new (Round.IccTransactions);
	}

	public virtual ITableExecution FindExecution(byte table)
	{
		if(Mcv.Users.Id == table)	return this;
		if(table == Mcv.Friends.Id)	return Friends;

		return null;
	}

	public virtual IBaseTableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Users.Id == table)		return FindUser(id as AutoId) != null ?		(IBaseTableEntry)AffectUser(id as AutoId) : null;
		if(Mcv.Friends.Id == table)		return Friends.Find(id as AutoId) != null ? (IBaseTableEntry)Friends.Affect(id as AutoId) : null;

		return null;
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Users)	return AffectedUsers;

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
		else if(!Round.AffectedMetas.TryGetValue(id, out a))
			a = Mcv.Metas.Find(id);
		
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

	public int IncrementMetaInt<E>(E type) where E : unmanaged, Enum
	{
		var m = AffectMeta(new MetaId(Unsafe.As<E, uint>(ref type)));

		if(m.Value == null)
		{
			m.Value = [1, 0, 0, 0];
			return 1;
		} 
		else
		{
			var i = BitConverter.ToInt32(m.Value) + 1;
			m.Value = BitConverter.GetBytes(i);
			return i;
		}
	}

	public int GetMetaInt<E>(E type) where E : unmanaged, Enum
	{
		var id = new MetaId(Unsafe.As<E, uint>(ref type));

		if(AffectedMetas.TryGetValue(id, out var a))
			return a.AsInt;

		if(Parent != null)
			Parent.AffectedMetas.TryGetValue(id, out a);
		else if(!Round.AffectedMetas.TryGetValue(id, out a))
			a = Mcv.Metas.Find(id);
		
		return a.AsInt;
	}

	//public int GetNextEid(TableBase table,  int b)
	//{
	//	int e = 0;
	//
	//	NextEids[table.Id].TryGetValue(b, out e);
	//
	//	if(e == 0)
	//		Round.NextEids[table.Id].TryGetValue(b, out e);
	//	
	//	if(e == 0)
	//		e = table.FindBucket(b)?.NextI ?? 0;
	//
	//	NextEids[table.Id][b] = e + 1;
	//
	//	return e;
	//}
	
	public void TransferEnergyIfNeeded(IEnergyHolder a)
	{
		var now = Time.Days/Net.ECLifetime.Days;

		if(a.EnergyThisPeriod != now)
		{
			if(a.EnergyThisPeriod + 1 == now) /// if this is next period only
				a.Energy = a.EnergyNext;
	
			a.EnergyNext = 0;
			a.EnergyThisPeriod	= (byte)now;
		}
	}

	public void PayOperationEnergy(IEnergyHolder spender)
	{
		PayEnergy(spender, (int)OperationCost);
	}

	public void PayEnergy(IEnergyHolder spender, int amount)
	{
		if(spender.EnergyPeriod < Time.Hours) /// switch to this hour
		{	
			if(spender.BandwidthExpiration < Time.Hours) /// bandwidth allocation expired
				spender.Bandwidth = 0;

			spender.EnergyPeriod	= Time.Hours;
			spender.EnergyRating	= spender.Bandwidth;
		}

		spender.EnergyRating -= amount;

		Transaction.EnergyConsumed += amount;
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

		var now = Time.Days;

		consumer.Space += space;

		if(consumer.Space > Net.FreeSpaceMaximum)
			payer.Free = false;
	
		var n = consumer.Expiration - now;

		if(!payer.Free)
		{	
			payer.Spacetime -= ToBD(space, (short)n);
			SpacetimeSpenders.Add(payer);
		}

		AffectSpaces();

		for(int i = 0; i < n; i++)
			Spaces[now + i] += space;
	}

	public void Prolong(ISpacetimeHolder payer, ISpaceConsumer consumer, Time duration)
	{	
		var now = Time.Days;
		var start = Math.Max(now, consumer.Expiration);

		consumer.Expiration = (short)(start + duration.Days);

		if(consumer.Expiration - now >= Time.FromYears(2).Days) /// 2 years of activity means 1 year prolongation maximum
			payer.Free = false;

		if(!payer.Free)
		{
			payer.Spacetime -= ToBD(consumer.Space, duration);
			SpacetimeSpenders.Add(payer);
		}

		var exp = start + duration.Days;

		if(exp > Spaces.Length)
			Spaces = [..Spaces, ..new long[exp - Spaces.Length]];
		else
			AffectSpaces(); /// needed below

		for(int i = start; i < exp; i++)
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
			if(!beneficiary.Free)
				beneficiary.Spacetime += ToBD(space, (short)(d - 1));
	
			AffectSpaces();
			
			for(int i = 0; i < d; i++)
				Spaces[now + i] -= space;
		}
	}

	public virtual User AffectSigner()
	{
 		if(Round.Id == 0)
 			return new User {Name = Mcv.GodName, Key = Mcv.God.Puplic};

		var name = Transaction.User;

		if(AffectedUsers.FirstOrDefault(i => i.Value.Name == name).Value is User u)
			return u;
		
		if(Parent != null)
			u = Parent.FindUser(name);
		else if(Round.AffectedUsers.Values.FirstOrDefault(i => i.Name == name) is User x)
			u = x;
		else
			u = Mcv.Users.Find(name);

		if(u == null)
		{	
//			if(!User.IsNameValid(name))
//			{
//				Transaction.Error = Operation.InvalidName;
//				return null;
//			}
//		
			u = CreateUser(name);
		}
		else
		{	
			if(Transaction.Signature != null && !Net.Cryptography.Verify(u.Key, Transaction.Hashify(Net), Transaction.Signature)) /// Transaction.Signature == null means synchronization
			{
				Transaction.Error = Operation.Denied;
				return null;
			}

			if(Transaction.Nonce != u.LastNonce + 1)
			{
				Transaction.Error = Operation.NotSequential;
				return null;
			}

			u = AffectUser(u.Id);
		}

		return u;
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

		if(Round.AffectedUsers.TryGetValue(id, out a))
			return a;

		return Mcv.Users.Find(id);
	}

	public User FindUser(string name)
	{
		if(AffectedUsers.Values.FirstOrDefault(i => i.Name == name) is User a)
			return a;

		if(Parent != null)
			return Parent.FindUser(name);

		if(Round.AffectedUsers.Values.FirstOrDefault(i => i.Name == name) is User x)
			return x;

		return Mcv.Users.Find(name);
	}

	public virtual User CreateUser(string name)
	{
		//var b = UserTable.KeyToBucket(name);
		//int e = GetNextEid(Mcv.Users, b);

		var a = Mcv.Users.Create();

		a.Id			= LastCreatedId = new AutoId(IncrementMetaInt(MetaEntityType.UserIdCounter));
		a.Name			= name;
		//a.Permissions	= [new Permission {Operations = [], Users = [a.Id]}];

		AffectedUsers[a.Id] = a;

		IncrementMetaInt(MetaEntityType.UserCount);

		return a;
	}

	public User AffectUser(AutoId id)
	{
		id = id == AutoId.LastCreated ? LastCreatedId : id;

		if(AffectedUsers.TryGetValue(id, out var a))
			return a;

		if(Parent != null)
			a = Parent.FindUser(id);
		else if(!Round.AffectedUsers.TryGetValue(id, out a))
			a = Mcv.Users.Find(id);

		a = a.Clone() as User;

		AffectedUsers[a.Id] = a;

		TransferEnergyIfNeeded(a);

		return a;
	}

	public Member AffectCandidate(AutoId generator)
	{
		_Candidates ??= [..Round.Candidates];

		var i = Candidates.FindIndex(i => i.Generator == generator);

		if(i == -1)
		{
			var c = Mcv.CreateGenerator();

			Candidates.Add(c);
		
			if(Candidates.Count > Mcv.Net.CandidatesMaximum)
				Candidates.RemoveAt(0);

			return c;
		}
		else
		{
			 return Candidates[i] = Candidates[i].Clone();
		}
	}
}

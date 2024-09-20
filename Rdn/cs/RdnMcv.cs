using System.Reflection;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public abstract class RdnCall<R> : McvCall<R> where R : PeerResponse
	{
		public new RdnNode	Node => base.Node as RdnNode;
		public RdnMcv		Rdn => Node.Mcv;
	}

	[Flags]
	public enum RdnRole : uint
	{
		None,
		Seed = 0b00000100,
	}

	public class RdnMcv : Mcv
	{
		public new RdnSettings			Settings => base.Settings as RdnSettings;
		public DomainTable				Domains;
		//public List<ForeignResult>	ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();

		static RdnMcv()
		{
			if(!ITypeCode.Contructors.ContainsKey(typeof(Operation)))
				ITypeCode.Contructors[typeof(Operation)] = [];

			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
			{
				ITypeCode.Codes[i] = (byte)Enum.Parse<RdnOperationClass>(i.Name);
				ITypeCode.Contructors[typeof(Operation)][(byte)Enum.Parse<RdnOperationClass>(i.Name)]  = i.GetConstructor([]);
			}

		}

		public RdnMcv()
		{
  		}

		public RdnMcv(McvZone zone, RdnSettings settings, string databasepath, bool skipinitload = false) : base(zone, settings, databasepath, skipinitload)
		{
		}

		public RdnMcv(RdnNode sun, RdnSettings settings, string databasepath, Flow flow, IClock clock) : base(sun, settings, databasepath, clock, flow)
		{
		}

		public override string CreateGenesis(AccountKey god, AccountKey f0)
		{
			/// 0	- declare F0
			/// P	- confirmed F0 membership
			/// P+P	- F0 start voting for P+P-P-1 = P-1

			Clear();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = GetRound(rid);
				r.ConsensusTransactions = r.OrderedTransactions.ToArray();
				r.Hashify();
				r.Write(w);
			}
	
			var v0 = CreateVote(); 
			{
				v0.RoundId = 0;
				v0.Time = Time.Zero;
				v0.ParentHash = Zone.Cryptography.ZeroHash;

				var t = new Transaction {Zone = Zone, Nid = 0, Expiration = 0};
				t.Generator = new([0, 0], -1);
				t.AddOperation(new UnitTransfer(f0, Zone.ECDayEmission, Zone.ECLifetime, Zone.BYDayEmission));
				t.Sign(god, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);

				t = new Transaction {Zone = Zone, Nid = 0, Expiration = 0};
				t.Generator = new([0, 0], -1);
				t.AddOperation(new RdnCandidacyDeclaration {BaseRdcIPs = [Zone.Father0IP], SeedHubRdcIPs = [Zone.Father0IP] });
				t.Sign(f0, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				v0.Sign(god);
				Add(v0);
				///v0.FundJoiners = v0.FundJoiners.Append(Zone.Father0).ToArray();
				write(0);
			}
	
			for(int i = 1; i <= LastGenesisRound; i++)
			{
				var v = CreateVote();
				v.RoundId	 = i;
				v.Time		 = Time.Zero;  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
				v.ParentHash = i < P ? Zone.Cryptography.ZeroHash : GetRound(i - P).Summarize();
		
				v.Sign(i < JoinToVote ? god : f0);
				Add(v);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		protected override void GenesisCreate(Vote vote)
		{
			//(vote as RdnVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			#if IMMISSION
			if(round.Id == 1)
				(round as RdnRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
			#endif
		}

		protected override void CreateTables(string databasepath)
		{
			var dbo	= new DbOptions().SetCreateIfMissing(true)
									 .SetCreateMissingColumnFamilies(true);

			var cfs = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{	new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (DomainTable.MetaColumnName,	new ()),
																new (DomainTable.MainColumnName,	new ()),
																new (DomainTable.MoreColumnName,	new ()),
																new (ChainFamilyName,				new ())})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Domains = new (this);

			Tables = [Accounts, Domains];
		}

		public override Operation CreateOperation(int type)
		{
			return	(typeof(Operation).Assembly.GetType(typeof(Operation).Namespace + "." + (RdnOperationClass)type)
					??
					typeof(RdnOperation).Assembly.GetType(typeof(RdnOperationClass).Namespace + "." + (RdnOperationClass)type))
					.GetConstructor([]).Invoke(null) as Operation;
		}

		public override Round CreateRound()
		{
			return new RdnRound(this);
		}

		public override Vote CreateVote()
		{
			return new RdnVote(this);
		}

		//public override Candidate CreateCandidate(Round round, CandidacyDeclaration declaration)
		//{
		//	return new RdnCandidate{Registered		= round.ConsensusTime,
		//							Account			= declaration.Signer.Id,
		//							BaseRdcIPs		= declaration.BaseRdcIPs, 
		//							SeedHubRdcIPs	= (declaration as RdnCandidacyDeclaration).SeedHubRdcIPs};
		//}

		public override Generator CreateGenerator()
		{
			return new RdnGenerator();
		}

		public override CandidacyDeclaration CreateCandidacyDeclaration()
		{
			return new RdnCandidacyDeclaration {//Pledge			= Settings.Pledge,
												BaseRdcIPs		= [Settings.Peering.IP],
												SeedHubRdcIPs	= [Settings.Peering.IP]};

		}

		public override void ClearTables()
		{
			Domains.Clear();
		}

		public IEnumerable<Resource> QueryResource(string query)
		{
			var r = Ura.Parse(query);
		
			var a = Domains.Find(r.Domain, LastConfirmedRound.Id);

			if(a == null)
				yield break;

			foreach(var i in a.Resources.Where(i => i.Address.Resource.StartsWith(r.Resource)))
				yield return i;
		}

		public override void FillVote(Vote vote)
		{
			var v = vote as RdnVote;

  			//v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

	}
}

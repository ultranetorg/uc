using System.Reflection;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public enum RdnOperationClass
	{
		None = 0, 
		RdnCandidacyDeclaration	= OperationClass.CandidacyDeclaration, 
		//Immission				= OperationClass.Immission, 
		UnitTransfer			= OperationClass.UnitTransfer, 

		DomainRegistration, DomainMigration, DomainBid, DomainUpdation,
		ResourceCreation, ResourceUpdation, ResourceDeletion, ResourceLinkCreation, ResourceLinkDeletion,
		AnalysisResultUpdation
	}

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
		public new RdnSettings		Settings => base.Settings as RdnSettings;
		public DomainTable			Domains;
		public List<ForeignResult>	ApprovedEmissions = new();
		public List<ForeignResult>	ApprovedMigrations = new();

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
			/// 0 - emission request
			/// 1 - vote for emission 
			/// 1+P	 - emited
			/// 1+P + 1 - candidacy declaration
			/// 1+P + 1+P - decalared
			/// 1+P + 1+P + P - joined

			Clear();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
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
				//t.EUFee = 1;
				//t.AddOperation(new Immission(Web3.Convert.ToWei(1_000_000, UnitConversion.EthUnit.Ether), 0));
				t.AddOperation(new UnitTransfer(f0, 10_000_000, 1000_000, 1000));
				t.Sign(god, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				v0.Sign(god);
				Add(v0);
				///v0.FundJoiners = v0.FundJoiners.Append(Zone.Father0).ToArray();
				write(0);
			}
			
			var v1 = CreateVote(); 
			{
				v1.RoundId = 1; 
				v1.Time = Time.Zero; 
				v1.ParentHash = Zone.Cryptography.ZeroHash;

				GenesisCreate(v1);
	
				v1.Sign(god);
				Add(v1);
				write(1);
			}
	
			for(int i = 2; i <= 1+P + 1+P + P; i++)
			{
				var v = CreateVote(); 	
				v.RoundId		= i;
				v.Time			= Time.Zero;  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
				v.ParentHash	= i < P ? Zone.Cryptography.ZeroHash : GetRound(i - P).Summarize();
		 
				if(i == 1+P + 1)
				{
					var t = new Transaction {Zone = Zone, Nid = 0, Expiration = i};
					t.Generator = new([0, 0], -1);
					//t.EUFee = 1;
					t.AddOperation(new RdnCandidacyDeclaration  {Pledge = 0,
																 BaseRdcIPs = [Zone.Father0IP],
																 SeedHubRdcIPs = [Zone.Father0IP] });
					t.Sign(f0, Zone.Cryptography.ZeroHash);
					v.AddTransaction(t);
				}
	
				v.Sign(god);
				Add(v);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		protected override void GenesisCreate(Vote vote)
		{
			(vote as RdnVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			if(round.Id == 1)
				(round as RdnRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
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

		public override Member CreateMember(Round round, CandidacyDeclaration declaration)
		{
			return new RdnMember{CastingSince	= round.Id + DeclareToGenerateDelay,
								 Bail			= declaration.Pledge,
								 Account		= declaration.Transaction.Signer, 
								 BaseRdcIPs		= declaration.BaseRdcIPs, 
								 SeedHubRdcIPs	= (declaration as RdnCandidacyDeclaration).SeedHubRdcIPs};
		}

		public override CandidacyDeclaration CreateCandidacyDeclaration()
		{
			return new RdnCandidacyDeclaration {Pledge			= Settings.Pledge,
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

  			v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

	}
}

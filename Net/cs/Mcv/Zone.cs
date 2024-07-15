using System;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public enum ZoneScope
	{
		None, 
		Local, 
		Developer0,
		Developer1,
		Developer2,
		Developer3,
		Developer4,
		Developer5,
		Developer6,
		Developer7,
		Developer8,
		Developer9,
		PublicTest
	}

	public abstract class Zone
	{
		public abstract string		Name { get; }
		public IPAddress[]			Initials;
		public Guid					Id;
		public abstract	ZoneScope	Scope { get; }
		public abstract ushort		BasePort { get; }
		public ushort				Port => (ushort)(ScopeBasePort + BasePort);

		public IPAddress[]			LocalInitials = Enumerable.Range(100, 16).Select(i => new IPAddress(new byte[] {127, 0, 0, (byte)i})).ToArray();
		public IPAddress[]			UOInitials = @"78.47.204.100 78.47.198.218 78.47.205.229 78.47.214.161 78.47.214.166 78.47.214.170 78.47.214.171
													 185.196.8.170 185.196.8.171 185.196.8.172 185.196.8.173 185.196.8.174
													 5.42.221.109 5.42.221.11 5.42.221.110 5.42.221.111 5.42.221.113 5.42.221.114
													 15.235.153.179 15.235.153.180 15.235.153.188 15.235.153.189 15.235.153.190 15.235.153.191
													 195.20.17.70 195.20.17.238 195.20.17.133 195.20.17.177 195.20.17.169 195.20.17.164 195.20.17.118 195.20.17.114
													 216.73.158.45 216.73.158.46 216.73.158.47
													 37.235.49.245 151.236.24.51 192.71.218.23 151.236.24.177
													 89.31.120.33 91.132.93.211 89.31.120.113 89.31.120.224
													 88.119.169.77 88.119.170.163 88.119.169.94 88.119.169.97
													 41.77.143.118 41.77.143.119 41.77.143.120 41.77.143.121
													45.159.250.254 77.91.75.32 77.91.75.34 77.91.75.225
													74.119.194.104 80.92.205.10 94.131.101.7 94.131.101.147
													 45.120.177.143 45.120.177.142 45.120.177.141 45.120.177.140 45.120.177.128 45.120.177.42 45.120.177.25
													 138.124.180.13 138.124.180.164 138.124.180.239"
												.Split(new char[] {'\r', '\n', '\t', ' '}, StringSplitOptions.RemoveEmptyEntries)
												.Select(i => IPAddress.Parse(i))
												.ToArray();

		public ushort				ScopeBasePort => Scope switch  {/// Format: XX000
																	ZoneScope.Local		 => 10000, 
																	ZoneScope.Developer0 => 20000,
																	ZoneScope.Developer1 => 21000,
																	ZoneScope.Developer2 => 22000,
																	ZoneScope.Developer3 => 23000,
																	ZoneScope.Developer4 => 24000,
																	ZoneScope.Developer5 => 25000,
																	ZoneScope.Developer6 => 26000,
																	ZoneScope.Developer7 => 27000,
																	ZoneScope.Developer8 => 28000,
																	ZoneScope.Developer9 => 29000,
																	ZoneScope.PublicTest		 => 30000,
																	_ => throw new IntegrityException()};
		public override string ToString()
		{
			return Name;
		}
	}

	public abstract class Interzone : Zone
	{
		public override ushort				BasePort => 010;
		public override	string				Name => Scope.ToString();

		public static readonly Interzone	Local = new LocalInterzone();
 		public static readonly Interzone	PublicTest = new PublicTestInterzone();
 		public static readonly Interzone	Developer0 = new Developer0Interzone();
		public static readonly Interzone	Main = null;
		public static readonly Interzone[]	Official = {Local, Developer0, PublicTest};

		public static Interzone				ByName(string name) => Official.First(i => i.Name == name);
		public static Interzone				Byid(Guid zoneid) => Official.First(i => i.Id == zoneid);

	}

	public class LocalInterzone : Interzone
	{
		public override	ZoneScope	Scope => ZoneScope.Local;

		public LocalInterzone()
		{
			Id			= new Guid("FFFFFFFF-0000-0000-0000-000000010010");
			Initials	= LocalInitials;
		}
	}

	public class Developer0Interzone : Interzone
	{
		public override	ZoneScope	Scope => ZoneScope.Developer0;

		public Developer0Interzone()
		{
			Id			= new Guid("FFFFFFFF-0000-0000-0000-000000020010");
			Initials	= UOInitials;
		}
	}

	public class PublicTestInterzone : Interzone
	{
		public override	ZoneScope	Scope => ZoneScope.PublicTest;

		public PublicTestInterzone()
		{
			Id			= new Guid("00000000-0000-0000-0000-000000030010");
			Initials	= UOInitials;
		}
	}

	public abstract class McvZone : Zone
	{
		public string			Genesis;	
 		public Cryptography		Cryptography									= Cryptography.Normal;
		public int				CommitLength									= 1000;
		public int				ExternalVerificationRoundDurationLimit			= 1000;
		public Unit				BailMin											= 1000;
		public bool				PoW												= false;
		public int				MembersLimit									= 1000;
		//public Money			ExeunitMinFee									= 0.001;
		//public long			TargetBaseGrowthPerYear							= 100L*1024*1024*1024;
		public int				TransactionsPerRoundLimit						= 5_000; /// for 5000 tx/sec signature recovering
		public int				TransactionsPerVoteAllowableOverflowMultiplier	= 10;
		public int				TransactionsOverflowFeeFactor					= 2;
		public int				OperationsPerTransactionLimit					= 100;
		public int				OperationsPerRoundLimit							=> TransactionsPerRoundLimit * OperationsPerTransactionLimit;
		public Unit				STCommitReward									= 1000;
		public Unit				EUCommitReward									= 1000;
		public Unit				EUCommitRewardOperationCountBelowTrigger		= 10_0000_000; /// 10`000 ops per round
		public Unit				MRCommitReward									= 1;

		public AccountAddress	God												= AccountAddress.Parse("0xFFFF9F9D0914ED338CB26CE8B1B9B8810BAFB608");
		public AccountAddress	Father0											= AccountAddress.Parse("0x0000A5A0591B2BF5085C0DDA2C39C5E478300C68");
		public IPAddress		Father0IP;
		public ZoneCreation		Creation;
	}
}

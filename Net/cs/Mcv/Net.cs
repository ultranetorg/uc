using System.Net;
using System.Reflection;

namespace Uccs.Net;

public enum Zone
{
	None, 
	Local		= 01, 
	Developer0	= 10,
	Developer1	= 11,
	Developer2	= 12,
	Developer3	= 13,
	Developer4	= 14,
	Developer5	= 15,
	Developer6	= 16,
	Developer7	= 17,
	Developer8	= 18,
	Developer9	= 19,
	_Public		= 20,
	PublicTest	= 21,
	Main		= 30
}

public enum KnownSystem
{
	//Ntn = 010,
	Rdn	= 020,
	Fair = 030,
	Uos = 900,
}

public abstract class Net
{
	public const string			Root = "rdn";

	public abstract string		Address { get; }
	public abstract string		Name { get; }
	//public Guid					Id;
	public abstract	Zone		Zone { get; }
	public abstract ushort		BasePort { get; }
	public ushort				Port => (ushort)((ushort)Zone * 1000 + BasePort);
	public ushort				NtnPort => (ushort)(Port + 1);

	public IPAddress[]			Initials;
	public IPAddress[]			LocalInitials = Enumerable.Range(100, 16).Select(i => new IPAddress([127, 0, 0, (byte)i])).ToArray();
	public IPAddress[]			UOInitials = "78.47.204.100 78.47.198.218 78.47.205.229 78.47.214.161 78.47.214.166 78.47.214.170 78.47.214.171 185.208.159.160 185.208.159.161 185.208.159.162 185.208.159.163 185.208.159.164 5.42.221.102 5.42.221.110 5.42.221.111 5.42.221.113 5.42.221.114 5.42.221.118 15.235.153.179 139.99.94.185 139.99.94.187 139.99.50.7 139.99.50.8 139.99.50.9 216.73.158.45 216.73.158.46 216.73.158.47 37.235.49.245 151.236.24.51 192.71.218.23 151.236.24.177 89.31.120.33 91.132.93.211 89.31.120.113 89.31.120.224 88.119.169.77 88.119.170.163 88.119.169.94 88.119.169.97 41.77.143.118 41.77.143.119 41.77.143.120 41.77.143.121 74.119.194.104 80.92.205.10 94.131.101.7 94.131.101.147 138.124.180.13 138.124.180.164 138.124.180.239"
											.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries)
											.Select(i => IPAddress.Parse(i))
											.ToArray();

	public Dictionary<Type, uint>								Codes = [];
	public Dictionary<Type, Dictionary<uint, ConstructorInfo>>	Contructors = [];

	public T				Contruct<T>(uint code) => (T)Contructors[typeof(T)][code].Invoke(null);

	public override string ToString()
	{
		return Address;
	}
}

//  	public class Ntn
//  	{
//  		public static ushort	BasePort => (ushort)KnownSystem.Ntn; /// 00XX0
// 
// 		public static ushort	GetPort(Zone land) => (ushort)((ushort)land * 1000 + BasePort);
//  	}
 
// 	public class LocalNexus : NetToNet
// 	{
// 		public override	NetScope	Scope => NetScope.Local;
// 
// 		public LocalNexus()
// 		{
// 			Initials	= [];
// 		}
// 	}
// 
// 	public class Developer0Nexus : NetToNet
// 	{
// 		public override	NetScope	Scope => NetScope.Developer0;
// 
// 		public Developer0Nexus()
// 		{
// 			Initials	= [];
// 		}
// 	}
// 
// 	public class PublicTestNexus : NetToNet
// 	{
// 		public override	NetScope	Scope => NetScope.PublicTest;
// 
// 		public PublicTestNexus()
// 		{
// 			Initials	= [];
// 		}
// 	}

public abstract class McvNet : Net
{
	public const long		IdealRoundsPerDay						= 60*60*24;
	public Time				ECLifetime								= Time.FromYears(1);

 	public Cryptography		Cryptography							= Cryptography.Normal;
	public int				CommitLength							= 1000;
	public int				ExternalVerificationRoundDurationLimit	= 1000;
	public bool				PoW										= false;
	public int				MembersLimit							= 1000;
	public long				CandidatesMaximum						= 1000 * 10;
	public long				TransactionsPerRoundAbsoluteLimit		= 15_000;
	public long				TransactionsPerRoundExecutionLimit		= 5_000; /// for 5000 tx/s signature recovering
	public long				OverloadFeeFactor						= 2;
	public int				ExecutionCyclesPerTransactionLimit		= 100;
	public long				ExecutionCyclesPerRoundMaximum			=> TransactionsPerRoundExecutionLimit * ExecutionCyclesPerTransactionLimit;
	public long				ECDayEmission							=> ExecutionCyclesPerRoundMaximum * IdealRoundsPerDay;
	public long				BYDayEmission							= 1024L * IdealRoundsPerDay;
	public long				ECEmission								=> ECDayEmission * 365;
	public long				DeclarationCost							=> 1000_000;
	
	public int				BandwidthAllocationDaysMaximum			=> 365;
	public long				BandwidthAllocationPerDayMaximum		=> ExecutionCyclesPerRoundMaximum * IdealRoundsPerDay / 2; /// 50%
	public long				BandwidthAllocationPerRoundMaximum		=> ExecutionCyclesPerRoundMaximum / 2; /// 50%

	public AccountAddress	God										= AccountAddress.Parse("0xFFFF9F9D0914ED338CB26CE8B1B9B8810BAFB608");
	public AccountAddress	Father0									= AccountAddress.Parse("0x0000A5A0591B2BF5085C0DDA2C39C5E478300C68");
	public IPAddress		Father0IP;
	public string			Genesis;	

	public McvNet()
	{
		Contructors[typeof(Operation)] = [];

		foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
		{
			Codes[i] = (uint)Enum.Parse<OperationClass>(i.Name);
			Contructors[typeof(Operation)][(uint)Enum.Parse<OperationClass>(i.Name)] = i.GetConstructor([]);
		}
	}
}

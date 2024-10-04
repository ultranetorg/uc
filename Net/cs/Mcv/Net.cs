using System.Net;
using System.Reflection;

namespace Uccs.Net
{
	public enum NetScope
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

	public abstract class Net
	{
		public abstract string		Name { get; }
		public Guid					Id;
		public abstract	NetScope	Scope { get; }
		public abstract ushort		BasePort { get; }
		public ushort				Port => (ushort)(ScopeBasePort + BasePort);

		public IPAddress[]			Initials;
		public IPAddress[]			LocalInitials = Enumerable.Range(100, 16).Select(i => new IPAddress(new byte[] {127, 0, 0, (byte)i})).ToArray();
		public IPAddress[]			UOInitials = @"78.47.204.100 78.47.198.218 78.47.205.229 78.47.214.161 78.47.214.166 78.47.214.170 78.47.214.171 185.208.159.160 185.208.159.161 185.208.159.162 185.208.159.163 185.208.159.164 5.42.221.109 5.42.221.11 5.42.221.110 5.42.221.111 5.42.221.113 5.42.221.114 15.235.153.179 15.235.153.180 15.235.153.188 15.235.153.189 15.235.153.190 15.235.153.191 195.20.17.70 195.20.17.238 195.20.17.133 195.20.17.177 195.20.17.169 195.20.17.164 195.20.17.118 195.20.17.114 216.73.158.45 216.73.158.46 216.73.158.47 37.235.49.245 151.236.24.51 192.71.218.23 151.236.24.177 89.31.120.33 91.132.93.211 89.31.120.113 89.31.120.224 88.119.169.77 88.119.170.163 88.119.169.94 88.119.169.97 41.77.143.118 41.77.143.119 41.77.143.120 41.77.143.121 74.119.194.104 80.92.205.10 94.131.101.7 94.131.101.147 138.124.180.13 138.124.180.164 138.124.180.239"
												.Split(new char[] {'\r', '\n', '\t', ' '}, StringSplitOptions.RemoveEmptyEntries)
												.Select(i => IPAddress.Parse(i))
												.ToArray();

		public ushort				ScopeBasePort => Scope switch  {/// Format: XX000
																	NetScope.Local		 => 10000, 
																	NetScope.Developer0 => 20000,
																	NetScope.Developer1 => 21000,
																	NetScope.Developer2 => 22000,
																	NetScope.Developer3 => 23000,
																	NetScope.Developer4 => 24000,
																	NetScope.Developer5 => 25000,
																	NetScope.Developer6 => 26000,
																	NetScope.Developer7 => 27000,
																	NetScope.Developer8 => 28000,
																	NetScope.Developer9 => 29000,
																	NetScope.PublicTest => 30000,
																	_ => throw new IntegrityException()};


		public Dictionary<Type, byte>								Codes = [];
		public Dictionary<Type, Dictionary<byte, ConstructorInfo>>	Contructors = [];

		public T				Contruct<T>(byte code) => (T)Contructors[typeof(T)][code].Invoke(null);

		public override string ToString()
		{
			return Name;
		}

		static Net()
		{
// 			if(!ITypeCode.Contructors.ContainsKey(typeof(PeerRequest)))
// 				ITypeCode.Contructors[typeof(PeerRequest)] = [];
// 
// 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsGenericType))
// 			{	
// 				if(Enum.TryParse<PeerCallClass>(i.Name.Remove(i.Name.IndexOf("Request")), out var c))
// 				{
// 					ITypeCode.Codes[i] = (byte)c;
// 					ITypeCode.Contructors[typeof(PeerRequest)][(byte)c] = i.GetConstructor([]);
// 				}
// 			}
// 	
// 			if(!ITypeCode.Contructors.ContainsKey(typeof(PeerResponse)))
// 				ITypeCode.Contructors[typeof(PeerResponse)] = [];
// 
// 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse))))
// 			{	
// 				if(Enum.TryParse<PeerCallClass>(i.Name.Remove(i.Name.IndexOf("Response")), out var c))
// 				{
// 					ITypeCode.Codes[i] = (byte)c;
// 					ITypeCode.Contructors[typeof(PeerResponse)][(byte)c]  = i.GetConstructor([]);
// 				}
// 			}
		}

	}

	public abstract class Nexus : Net
	{
		public override ushort			BasePort => 010; /// 00XX0
		public override	string			Name => Scope.ToString();

		public static readonly Nexus	Local = new LocalNexus();
 		public static readonly Nexus	PublicTest = new PublicTestNexus();
 		public static readonly Nexus	Developer0 = new Developer0Nexus();
		public static readonly Nexus	Main = null;
		public static readonly Nexus[]	Official = {Local, Developer0, PublicTest};

		public static Nexus				ByName(string name) => Official.First(i => i.Name == name);
		public static Nexus				Byid(Guid id) => Official.First(i => i.Id == id);

		public abstract Guid			DefaultRdn { get; }
	}

	public class LocalNexus : Nexus
	{
		public override	NetScope	Scope => NetScope.Local;
		public override Guid		DefaultRdn => new Guid("FFFFFFFF-1002-0000-0000-000000000000");

		public LocalNexus()
		{
			Id			= new Guid("FFFFFFFF-1001-0000-0000-000000000000");
			Initials	= LocalInitials;
		}
	}

	public class Developer0Nexus : Nexus
	{
		public override	NetScope	Scope => NetScope.Developer0;
		public override Guid		DefaultRdn => new Guid("FFFFFFFF-2002-0000-0000-000000000000");

		public Developer0Nexus()
		{
			Id			= new Guid("FFFFFFFF-2001-0000-0000-000000000000");
			Initials	= UOInitials;
		}
	}

	public class PublicTestNexus : Nexus
	{
		public override	NetScope	Scope => NetScope.PublicTest;
		public override Guid		DefaultRdn => new Guid("30020000-0000-0000-0000-000000000000");

		public PublicTestNexus()
		{
			Id			= new Guid("30010000-0000-0000-0000-000000000000");
			Initials	= UOInitials;
		}
	}

	public abstract class McvNet : Net
	{
		public const long		IdealRoundsPerDay						= 60*60*24;
		public Time				ECLifetime { get; protected set; }		= Time.FromYears(1);

		public string			Genesis;	
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
		public NetCreation		Creation;

		static McvNet()
		{
// 			if(!ITypeCode.Contructors.ContainsKey(typeof(PeerRequest)))
// 				ITypeCode.Contructors[typeof(PeerRequest)] = [];
// 
// 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsGenericType))
// 			{	
// 				if(Enum.TryParse<McvPeerCallClass>(i.Name.Remove(i.Name.IndexOf("Request")), out var c))
// 				{
// 					ITypeCode.Codes[i] = (byte)c;
// 					ITypeCode.Contructors[typeof(PeerRequest)][(byte)c]  = i.GetConstructor([]);
// 				}
// 			}
// 	
// 			if(!ITypeCode.Contructors.ContainsKey(typeof(PeerResponse)))
// 				ITypeCode.Contructors[typeof(PeerResponse)] = [];
// 
// 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse))))
// 			{	
// 				if(Enum.TryParse<McvPeerCallClass>(i.Name.Remove(i.Name.IndexOf("Response")), out var c))
// 				{
// 					ITypeCode.Codes[i] = (byte)c;
// 					ITypeCode.Contructors[typeof(PeerResponse)][(byte)c]  = i.GetConstructor([]);
// 				}
// 			}
// 
//			if(!ITypeCode.Contructors.ContainsKey(typeof(Operation)))
//				ITypeCode.Contructors[typeof(Operation)] = [];
//
//			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
//			{
//				ITypeCode.Codes[i] = (byte)Enum.Parse<OperationClass>(i.Name);
//				ITypeCode.Contructors[typeof(Operation)][(byte)Enum.Parse<OperationClass>(i.Name)]  = i.GetConstructor([]);
//			}
		}

		public McvNet()
		{
			Contructors[typeof(Operation)] = [];

			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
			{
				Codes[i] = (byte)Enum.Parse<OperationClass>(i.Name);
				Contructors[typeof(Operation)][(byte)Enum.Parse<OperationClass>(i.Name)] = i.GetConstructor([]);
			}
		}
	}
}

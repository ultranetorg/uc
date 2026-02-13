using System.Net;
using System.Reflection;

namespace Uccs.Net;

public enum Zone : ushort
{
	None, 
	Local		= 02_00_0, 
	TA			= 03_00_0,
	Main		= 10_00_0,
	Test		= 11_00_0,
	Developer0	= 12_00_0,
}

public enum KnownProtocol : ushort
{
	Nni			= 000,
	Rdn			= 001,
	Fair		= 002,
	Api			= 900,
}

public abstract class Net
{
	public const string					Root = "rdn";
	public readonly static IPAddress	DefaultHost = new IPAddress([127, 1, 0, 0]);

	public abstract string				Address { get; }
	public abstract string				Name { get; }
	public abstract	Zone				Zone { get; }
	public abstract ushort				PpiPort { get; }
	public ushort						NniPort => MapPort(Zone, KnownProtocol.Nni);

	public IPAddress[]					Initials;
	public IPAddress[]					LocalInitials = Enumerable.Range(0, 16).Select(i => new IPAddress([127, 1, 0, (byte)i])).ToArray();
	public IPAddress[]					UOInitials = @" 78.47.204.100	
														185.208.159.160	
														5.42.221.102	
														139.99.94.185	
														216.73.158.45	
														37.235.49.245	
														89.31.120.33	
														88.119.169.77	
														41.77.143.118	
														74.119.194.104	
														138.124.180.13	"
													.Split(['\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries)
													.Select(i => IPAddress.Parse(i))
													.ToArray();

	public Constructor					Constructor = new ();

	public static ushort				MapPort(Zone zone, KnownProtocol system) => (ushort)(zone + (ushort)system);

	public override string ToString()
	{
		return Address;
	}

	public static string Escape(string path)
	{
		return new char[] {' '}.Concat(Path.GetInvalidFileNameChars()).Aggregate(path.ToString(), (c1, c2) => c1.Replace(c2.ToString(), $" {(short)c2} "));
	}

	public static string Unescape(string path)
	{
		return new char[] {' '}.Concat(Path.GetInvalidFileNameChars()).Aggregate(path, (c1, c2) => c1.Replace($" {(short)c2} ", c2.ToString()));
	}

}

public abstract class McvNet : Net
{
	public const long						IdealRoundsPerDay						= 60*60*24;
	public const int						BandwidthRentMonthsMaximum				= 12;
	public const int						BandwidthPeriodsMaximum					= BandwidthRentMonthsMaximum * 30 * 24;
	public virtual int						FreeSpaceMaximum						=> 0;
	public Time								ECLifetime								= Time.FromYears(1);
	public ushort							UserFreeCreationPoWDifficulity			= 172;

 	public Cryptography						Cryptography							= Cryptography.Mcv;
	public int								CommitLength							= 1000;
	public int								AffectedCountMaximum					= 100_000;
	public int								ExternalVerificationRoundDurationLimit	= 1000;
	public int								MembersLimit							= 1000;
	public long								CandidatesMaximum						= 1000 * 10;
	
	public long								TransactionsPerRoundMaximum				= 100_000;
	public long								OperationsPerRoundMaximum				= 100_000;
	//public long							OverloadFeeFactor						= 2;
	
	public int								ExecutionCyclesPerTransactionLimit		= 200;
	//public long								ExecutionCyclesPerRoundMaximum			=> TransactionsPerRoundExecutionLimit * ExecutionCyclesPerTransactionLimit;
	public long								EnergyDailyEmission						=> OperationsPerRoundMaximum * IdealRoundsPerDay;
	public long								EnergyHourlyEmission					=> OperationsPerRoundMaximum * IdealRoundsPerDay/24;
	public long								EnergyEmission							=> EnergyDailyEmission * 365;
	public long								SpacetimeDayEmission					= 1024L*1024L * IdealRoundsPerDay;
	public long								DeclarationCost							=> 1000_000;
	
	public int								EntityLength							= 100;

	//public long								BandwidthDayCapacity					=> ExecutionCyclesPerRoundMaximum * IdealRoundsPerDay;
	//public long								BandwidthAllocationPerRoundMaximum		=> ExecutionCyclesPerRoundMaximum / 2; /// 50%

	public abstract int						TablesCount { get; }

	public Endpoint							Father0IP;
	public readonly string					Father0Name		= "father0000";
	public readonly AutoId					Father0Id		= new AutoId(287078, 0);
	public readonly AccountAddress			Father0Signer	= AccountAddress.Parse("0x0000A5A0591B2BF5085C0DDA2C39C5E478300C68");

	public McvNet()
	{
		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(OperationClass), i => i);
	}
}

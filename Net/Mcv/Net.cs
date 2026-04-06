using System.Net;
using System.Reflection;

namespace Uccs.Net;

public enum Zone : ushort
{
	None, 
	Simulation	= 02_00_0, 
	Virtual		= 03_00_0, 
	TA			= 04_00_0,
	Main		= 10_00_0,
	Test		= 11_00_0,
	Developer0	= 12_00_0,
}

public enum KnownProtocol : ushort
{
	Nni			= 000,
	SystemApi	= 900,
	Rdn			= 001,
	RdnApi		= 901,
	Fair		= 002,
	FairApi		= 902,
	//Api			= 900,
}

public class Port
{
	public static ushort	Map(Zone zone, KnownProtocol system) => (ushort)(zone + (ushort)system);
}

public abstract class Net
{
	public const string					Root = "rdn";
	public readonly static IPAddress	DefaultHost = new IPAddress([127, 1, 0, 0]);

	public abstract string				Address { get; }
	public abstract string				Name { get; }
	public abstract	Zone				Zone { get; }
	public abstract ushort				PpiPort { get; }
	public abstract ushort				ApiPort { get; }
	public ushort						NniPort => Port.Map(Zone, KnownProtocol.Nni);

	public IPAddress[]					Initials;
	public static readonly  IPAddress[]	LocalInitials = Enumerable.Range(0, 16).Select(i => new IPAddress([127, 1, 0, (byte)i])).ToArray();
	public static readonly  IPAddress[]	UOInitials = @" 78.47.204.100	
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

	public static readonly  IPAddress[]	VirtualInitials	= [	new([192, 168, 88, 100]),
															new([192, 168, 88, 101]),
															new([192, 168, 88, 102]),
															new([192, 168, 88, 103]),
															new([192, 168, 88, 104]),
															new([192, 168, 88, 105]),
															new([192, 168, 88, 106]),
															new([192, 168, 88, 107]),
															new([192, 168, 88, 108]),
															new([192, 168, 88, 109]),
															new([192, 168, 88, 110]),
															new([192, 168, 88, 111]),
															new([192, 168, 88, 112]),
															new([192, 168, 88, 113]),
															new([192, 168, 88, 114]),
															new([192, 168, 88, 115]),
															];

	public Constructor					Constructor = new ();


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
	public const long						IdealRoundsPerSecond					= 1;
	public const long						IdealRoundsPerDay						= IdealRoundsPerSecond * 60*60*24;
	public const int						BandwidthRentMonthsMaximum				= 12;
	public const int						BandwidthPeriodsMaximum					= BandwidthRentMonthsMaximum * 30 * 24;
	public virtual int						FreeSpaceMaximum						=> 0;
	public Time								ECLifetime								= Time.FromYears(1);
	public ushort							UserCreationPoWDifficulity				= 172;
	public int								EntityLength							= 100;

 	public Cryptography						Cryptography							= Cryptography.Mcv;
	public int								AffectedCountMaximum					= 100_000;
	public Time								ForeignVerificationDurationLimit		= Time.FromHours(1);
	public int								MembersLimit							= 1000;
	public long								CandidatesMaximum						= 1000 * 10;
	
	public long								TransactionsPerRoundMaximum				= 100_000;
	public long								OperationsPerRoundMaximum				= 100_000;
	
	public int								ExecutionCyclesPerTransactionLimit		= 200; /// Not more than 256, see OperationId.Oi
	//public long								ExecutionCyclesPerRoundMaximum			=> TransactionsPerRoundExecutionLimit * ExecutionCyclesPerTransactionLimit;
	public long								EnergyDailyEmission						=> OperationsPerRoundMaximum * IdealRoundsPerDay;
	public long								EnergyHourlyEmission					=> OperationsPerRoundMaximum * IdealRoundsPerDay/24;
	public long								EnergyEmission							=> EnergyDailyEmission * 365;
	public long								SpacetimeDayEmission					= 1024L*1024L * IdealRoundsPerDay;
	public long								DeclarationCost							=> 1000_000;


	public Endpoint							Father0EP;
	public readonly string					Father0Name		= "f000";
	public readonly AutoId					Father0Id;
	public readonly AccountAddress			Father0Signer	= new ("0000000AD6AFF35CF87E04E457A9395EAB7397D335C5B530F8CDBC9BD66EDF4D".FromHex());

	public abstract int						TablesCount { get; }

	public McvNet()
	{
		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(OperationClass), i => i);

		Father0Id = new (UserTable.KeyToBucket(Father0Name), 0);
	}
}

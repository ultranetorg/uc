using System.Net;
using System.Reflection;

namespace Uccs.Fair;

public abstract class Fair : McvNet
{
	public override	string						Address => "fair.rdn";
	public override	string						Name => "fair";
	public override ushort						PpiPort => Port.Map(Zone, KnownProtocol.Fair);
	public override ushort						ApiPort => Port.Map(Zone, KnownProtocol.FairApi);
	public int									OutwardVerificationEnergyCost => 100;
	public override Dictionary<string, byte>	Tables => Enum.GetValues<FairTable>().Where(i => i.ToString()[0] != '_').ToDictionary(i => i.ToString(), i => (byte)i);

	public int									FileLengthMaximum = 1024*1024;
	public const ushort							PostLengthMaximum = 65535;
	public ushort								NicknameLengthMaximum = 32;
	public const ushort							TitleLengthMaximum = 64;
	public const ushort							SloganLengthMaximum = 128;
 		
	public ushort								WebPort = 1080;

	public static readonly IPAddress[]			LocalInitials = Enumerable.Range(100, 16).Select(i => new IPAddress([127, 1, 0, (byte)i])).ToArray();

 	public static Fair							Simulated = new SimulationFair();
 	public static readonly Fair					Virtual = new VirtualFair();
 	public static readonly Fair					Test = new TestFair();
 	public static readonly Fair					Developer0 = new Developer0Fair();
 	public static readonly Fair					TA = new TaFair();
	public static readonly Fair					Main = null;

	public static Fair							ByZone(Zone name) => new Fair[]{Simulated, Virtual, Developer0, Test, TA}.First(i => i.Zone == name);
	
	public Fair()
	{
		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(FairOperationClass), i => i);
	}
}

public class SimulationFair : Fair
{	
	public override	Zone	Zone => Zone.Simulation;
	
	public SimulationFair()
	{
		Initials					= LocalInitials;
		Father0EP					= new (LocalInitials[0], PpiPort);
		Cryptography				= Cryptography.No;
		AffectedCountMaximum		= 10;
		ECLifetime					= Time.FromYears(100);
		UserCreationPoWDifficulity	= 0;

	}
}

public class VirtualFair : Fair
{
	public override	Zone	Zone => Zone.Virtual;

	public VirtualFair()
	{
		Initials					= VirtualInitials;
 		Father0EP					= new(Initials[0], PpiPort);
		UserCreationPoWDifficulity	= 0;
	}
}

public class Developer0Fair : Fair
{
	public override	Zone		Zone => Zone.Developer0;

	public Developer0Fair()
	{
		var z = Test;

		Initials	= z.Initials;
 		Father0EP	= z.Father0EP;
	}
}

public class TestFair : Fair
{
	public override	Zone		Zone => Zone.Test;

	public TestFair()
	{
		Initials	= UOInitials;
 		Father0EP	= new (Initials[0], PpiPort);
	}
}

public class TaFair : Fair
{	
	public override	Zone	Zone => Zone.TA;
	
	public TaFair()
	{
		Initials					= LocalInitials;
		Father0EP					= new (LocalInitials[0], PpiPort);
		UserCreationPoWDifficulity	= 0;
	}
}

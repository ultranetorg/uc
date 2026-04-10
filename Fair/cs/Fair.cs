using System.Net;
using System.Reflection;

namespace Uccs.Fair;

public abstract class Fair : McvNet
{
	public override	string			Address => "fair";
	public override	string			Name => "fair";
	public override ushort			PpiPort => Port.Map(Zone, KnownProtocol.Fair);
	public override ushort			ApiPort => Port.Map(Zone, KnownProtocol.FairApi);
	public override int				TablesCount => Enum.GetNames<FairTable>().Count(i => i[0] != '_');

	public int						FileLengthMaximum = 1024*1024;
	public const ushort				PostLengthMaximum = 65535;
	public ushort					NicknameLengthMaximum = 32;
	public const ushort				TitleLengthMaximum = 64;
	public const ushort				SloganLengthMaximum = 128;
 		
	public ushort					WebPort = 1080;

	public static Dictionary<Type, uint>								OCodes = [];
	public static Dictionary<Type, Dictionary<uint, ConstructorInfo>>	OContructors = [];

 	public static Fair				Simulated = new SimulationFair();
 	public static readonly Fair		Virtual = new VirtualFair();
 	public static readonly Fair		Test = new TestFair();
 	public static readonly Fair		Developer0 = new Developer0Fair();
 	public static readonly Fair		TA = new TaFair();
	public static readonly Fair		Main = null;

	public static Fair				ByZone(Zone name) => new Fair[]{Simulated, Virtual, Developer0, Test, TA}.First(i => i.Zone == name);

	
	public Fair()
	{
		OContructors[typeof(Operation)] = [];

		foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
		{
			if(Enum.TryParse<FairOperationClass>(i.Name, out var v))
			{
				OCodes[i] = (uint)v;
				OContructors[typeof(Operation)][(uint)v] = i.GetConstructor([]);
			}
		}

		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(FairOperationClass), i => i);
	}
}

public class SimulationFair : Fair
{	
	public override	Zone	Zone => Zone.Simulation;
	
	public SimulationFair()
	{
		Father0EP					= new (DefaultHost, PpiPort);
		Cryptography				= Cryptography.No;
		AffectedCountMaximum		= 10;
		ECLifetime					= Time.FromYears(100);
		UserCreationPoWDifficulity	= 0;

		Initials					= LocalInitials;
	}
}

public class VirtualFair : Fair
{
	public override	Zone	Zone => Zone.Virtual;

	public VirtualFair()
	{
 		Father0EP					= new(VirtualInitials[0], PpiPort);
		Initials					= VirtualInitials;
		UserCreationPoWDifficulity	= 0;
	}
}

public class Developer0Fair : Fair
{
	public override	Zone	Zone => Zone.Developer0;

	public Developer0Fair()
	{
		var z = Test;

 		Father0EP	= z.Father0EP;
		Initials	= z.Initials;
	}
}

public class TestFair : Fair
{
	public override	Zone	Zone => Zone.Test;

	public TestFair()
	{
 		Father0EP	= new (IPAddress.Parse("78.47.204.100"), PpiPort);
		Initials	= UOInitials;
	}
}

public class TaFair : Fair
{	
	public override	Zone	Zone => Zone.TA;
	
	public TaFair()
	{
		Father0EP					= new (DefaultHost, PpiPort);
		Initials					= LocalInitials;
		UserCreationPoWDifficulity	= 0;
	}
}

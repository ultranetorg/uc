using System.Net;
using System.Reflection;

namespace Uccs.Rdn;

public abstract class Rdn : McvNet
{
	public override	string			Address => Root;
	public override	string			Name => Root;
	public override ushort			PpiPort => Port.Map(Zone, KnownProtocol.Rdn);
	public override ushort			ApiPort => Port.Map(Zone, KnownProtocol.RdnApi);
	public override int				TablesCount => Enum.GetValues<RdnTable>().Length;
	public override int				FreeSpaceMaximum => 4096;
	public int						FreeNameLengthMinimum => 8;
	public int						CircularDependeciesChecksMaximum => 100_000;

 	public static readonly Rdn		Simulated = new SimulationRdn();
 	public static readonly Rdn		Virtual = new VirtualRdn();
 	public static readonly Rdn		Test = new TestRdn();
 	public static readonly Rdn		Developer0 = new Developer0Rdn();
 	public static readonly Rdn		TA = new TaRdn();
	public static readonly Rdn		Main = null;

	public static Rdn				ByZone(Zone zone) => new Rdn[]{Simulated, Virtual, Developer0, Test, TA}.First(i => i.Zone == zone);
	//public bool						IsFree(Domain domain) => domain.Space <= FreeSpaceMaximum && domain.Address.Length >= FreeNameLengthMinimum;

	public Rdn()
	{
		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(RdnOperationClass), i => i, overwrite: true);
	}
}

public class SimulationRdn : Rdn
{	
	public override	Zone	Zone => Zone.Simulation;
	
	public SimulationRdn()
	{
		Father0EP					= new(DefaultHost, PpiPort);
		Cryptography				= Cryptography.No;
		AffectedCountMaximum		= 10;
		ECLifetime					= Time.FromYears(100);
		UserCreationPoWDifficulity	= 0;

		Initials					= LocalInitials;
	}
}

public class VirtualRdn : Rdn
{
	public override	Zone	Zone => Zone.Virtual;

	public VirtualRdn()
	{
 		Father0EP					= new(VirtualInitials[0], PpiPort);
		Initials					= VirtualInitials;
		UserCreationPoWDifficulity	= 0;
	}
}
public class Developer0Rdn : Rdn
{
	public override	Zone	Zone => Zone.Developer0;

	public Developer0Rdn()
	{
		var z = Test;

 		Father0EP	= z.Father0EP;
		Initials	= z.Initials;
	}
}

public class TestRdn : Rdn
{
	public override	Zone	Zone => Zone.Test;

	public TestRdn()
	{
 		Father0EP	= new(IPAddress.Parse("78.47.204.100"), PpiPort);
		Initials	= UOInitials;
	}
}


public class TaRdn : Rdn
{	
	public override	Zone	Zone => Zone.TA;
	
	public TaRdn()
	{
		Father0EP						= new(DefaultHost, PpiPort);
		Initials						= LocalInitials;
		UserCreationPoWDifficulity	= 0;
	}
}

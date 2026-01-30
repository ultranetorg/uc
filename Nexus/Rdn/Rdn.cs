using System.Net;
using System.Reflection;

namespace Uccs.Rdn;

public abstract class Rdn : McvNet
{
	public override	string			Address => Root;
	public override	string			Name => Root;
	public override ushort			PpiPort => MapPort(Zone, KnownProtocol.Rdn);
	public override int				TablesCount => Enum.GetValues<RdnTable>().Length;
	public override int				FreeSpaceMaximum => 4096;
	public int						FreeNameLengthMinimum => 8;
	public int						CircularDependeciesChecksMaximum => 100_000;
		
 	public static readonly Rdn		Local = new RdnLocal();
 	public static readonly Rdn		Test = new RdnTest();
 	public static readonly Rdn		Developer0 = new RdnDeveloper0();
 	public static readonly Rdn		TA = new RdnTA();
	public static readonly Rdn		Main = null;

	public static Rdn				ByZone(Zone zone) => new Rdn[]{Local, Developer0, Test, TA}.First(i => i.Zone == zone);
	//public bool						IsFree(Domain domain) => domain.Space <= FreeSpaceMaximum && domain.Address.Length >= FreeNameLengthMinimum;

	public Rdn()
	{
		Constructor.Register<Operation>(Assembly.GetExecutingAssembly(), typeof(RdnOperationClass), i => i, overwrite: true);
	}
}

public class RdnLocal : Rdn
{	
	public override	Zone	Zone => Zone.Local;
	
	public RdnLocal()
	{
		Father0IP						= new(DefaultHost, PpiPort);
		Cryptography					= Cryptography.No;
		CommitLength					= 100;
		ECLifetime						= Time.FromYears(100);
		UserFreeCreationPoWDifficulity	= 150;

		Initials						= LocalInitials;
	}
}

public class RdnDeveloper0 : Rdn
{
	public override	Zone	Zone => Zone.Developer0;

	public RdnDeveloper0()
	{
		var z = Test;

 		Father0IP	= z.Father0IP;
		Initials	= z.Initials;
	}
}

public class RdnTest : Rdn
{
	public override	Zone	Zone => Zone.Test;

	public RdnTest()
	{
 		Father0IP	= new(IPAddress.Parse("78.47.204.100"), PpiPort);
		Initials	= UOInitials;
	}
}

public class RdnTA : Rdn
{	
	public override	Zone	Zone => Zone.TA;
	
	public RdnTA()
	{
		Father0IP		= new(DefaultHost, PpiPort);
		Initials		= LocalInitials;
	}
}

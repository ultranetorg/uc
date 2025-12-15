using System.Net;
using System.Reflection;

namespace Uccs.Fair;

public abstract class Fair : McvNet
{
	public override	string			Address => "fair";
	public override	string			Name => "fair";
	public override ushort			PpiPort => MapPort(Zone, KnownProtocol.Fair);
	public override int				TablesCount => Enum.GetNames<FairTable>().Count(i => i[0] != '_');
	
	public int						FileLengthMaximum = 1024*1024;
	public const ushort				PostLengthMaximum = 65535;
	public ushort					NicknameLengthMaximum = 32;
	public const ushort				TitleLengthMaximum = 64;
	public const ushort				SloganLengthMaximum = 128;
 		
	public static Dictionary<Type, uint>								OCodes = [];
	public static Dictionary<Type, Dictionary<uint, ConstructorInfo>>	OContructors = [];

 	public static readonly Fair		Local = new FairLocal();
 	public static readonly Fair		Test = new FairTest();
 	public static readonly Fair		Developer0 = new FairDeveloper0();
 	public static readonly Fair		TA = new FairTA();
	public static readonly Fair		Main = null;

	public static Fair				ByZone(Zone name) => new Fair[]{Local, Developer0, Test, TA}.First(i => i.Zone == name);

	
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

public class FairLocal : Fair
{	
	public override	Zone	Zone => Zone.Local;
	
	public FairLocal()
	{
		Father0IP		= new (DefaultHost, PpiPort);
		Cryptography	= Cryptography.No;
		CommitLength	= 100;
		ECLifetime		= Time.FromYears(100);
		//PoWComplexity	= 0;

		Initials		= LocalInitials;
	}
}

public class FairDeveloper0 : Fair
{
	public override	Zone	Zone => Zone.Developer0;

	public FairDeveloper0()
	{
		var z = Test;

 		Father0IP	= z.Father0IP;
		Initials	= z.Initials;
	}
}

public class FairTest : Fair
{
	public override	Zone	Zone => Zone.Test;

	public FairTest()
	{
 		Father0IP	= new (IPAddress.Parse("78.47.204.100"), PpiPort);
		Initials	= UOInitials;
	}
}

public class FairTA : Fair
{	
	public override	Zone	Zone => Zone.TA;
	
	public FairTA()
	{
		Father0IP		= new (DefaultHost, PpiPort);
		Initials		= LocalInitials;
	}
}

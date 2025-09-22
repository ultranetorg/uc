using System.Net;
using System.Reflection;

namespace Uccs.Rdn;

public abstract class Rdn : McvNet
{
	public override	string			Address => Root;
	public override	string			Name => Root;
	public override ushort			PortBase => (ushort)KnownSystem.Rdn;
	public override int				TablesCount => Enum.GetValues<RdnTable>().Length;
 		
	public bool						Auctions				= false;
	public long						DomainRankCheckECFee	= 5;

 	public static readonly Rdn		Local = new RdnLocal();
 	public static readonly Rdn		Test = new RdnTest();
 	public static readonly Rdn		Developer0 = new RdnDeveloper0();
	public static readonly Rdn		Main = null;
	public static readonly Rdn[]	Official = [Local, Developer0, Test];

	public static Rdn				ByZone(Zone zone) => Official.First(i => i.Zone == zone);
	
	public Rdn()
	{
		foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
		{
			if(Enum.TryParse<RdnOperationClass>(i.Name, out var v))
			{
				Codes[i] = (uint)v;
				Contructors[typeof(Operation)][(uint)v]  = i.GetConstructor([]);
			}
		}
	}
}

public class RdnLocal : Rdn
{	
	public override	Zone	Zone => Zone.Local;
	
	public RdnLocal()
	{
		Father0IP		= new IPAddress([127, 1, 0, 0]);
		Cryptography	= Cryptography.No;
		Auctions		= false;
		CommitLength	= 100;
		ECLifetime		= Time.FromYears(100);

		Initials		= LocalInitials;
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
 		Father0IP	= IPAddress.Parse("78.47.204.100");
		Initials	= UOInitials;
	}
}

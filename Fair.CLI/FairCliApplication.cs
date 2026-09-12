using System.Reflection;

namespace Uccs.Fair.CLI;

public class FairCliApplication
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new FairCli();
	}
}

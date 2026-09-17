namespace Uccs.Fair.CLI;

public class FairCla
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new FairCli();
	}
}

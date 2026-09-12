public class RdnCliApplication
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new Uccs.Rdn.CLI.RdnCli();
	}
}

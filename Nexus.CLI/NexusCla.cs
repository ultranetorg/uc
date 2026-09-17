public class NexusCla
{
	public static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		new Uccs.Nexus.CLI.NexusCli();
	}
}	

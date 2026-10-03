namespace Uccs.Fair.CLI;

public class FairCla
{
	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		Console.InputEncoding =
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		new FairCli();
	}
}

using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class FileCommand : FairCommand
{
	AutoId First => AutoId.Parse(Args[0].Name);

	public FileCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		var path = "path";

		a.Name = "c";
		a.Description = "Creates a file entity in the MCV database";
		a.Arguments =	[
							new (null, EA, "An entity address of file owner", Flag.First),
							new (path, PATH, "A data associated with the file"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return	new FileCreation
										{
											Owner = EntityAddress.Parse<FairTable>(Args[0].Name), 
											Data = System.IO.File.ReadAllBytes(GetString(path))
										};
							};
		return a;
	}
		
	public CommandAction Destroy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Description = "Destroys existing file and all its associated data";
		a.Arguments = [new (null, EID, "Id of a file to delete", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new FileDeletion {File = First};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Gets file entity information from the MCV database";
		a.Arguments = [new (null, EID, "Id of a file to get information about", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Ppc(new FilePpc(First)).File;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
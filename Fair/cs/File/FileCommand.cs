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
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		var path = "path";

		a.Name = "c";
		a.Help = new()
				 {
					Description = "Creates a file entity in the MCV database",
					Arguments = [
									new (null, EA, "An entity address of file owner", Flag.First),
									new (path, PATH, "A data associated with the file"),
									SignerArgument()
								],
				 };

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return	new FileCreation
										{
											Owner = EntityAddress.Parse(Args[0].Name), 
											Data = System.IO.File.ReadAllBytes(GetString(path))
										};
							};
		return a;
	}
		
	public CommandAction Destroy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Help = new()
				 {
					Description = "Destroys existing file and all its associated data",
					Arguments = [new (null, EID, "Id of a file to delete", Flag.First)],
				 };

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new FileDeletion {File = First};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new()
				 {
					Description = "Gets file entity information from the MCV database",
					Arguments = [new (null, EID, "Id of a file to get information about", Flag.First)],
				 };

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Ppc(new FileRequest(First)).File;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
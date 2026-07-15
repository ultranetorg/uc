using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class FileCommand : FairCommand
{
	public FileCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string owner = nameof(owner);
		const string path = nameof(path);
		const string mime = nameof(mime);

		a.Description = "Creates a file entity";
		a.Arguments =	[
							new (owner, EA, "Entity address of the file owner"),
							new (path, PATH, "Path to the file content"),
							new (mime, TEXT, "Mime type of the file content", ArgumentFlag.Optional),
							ByArgument("Name of the user authorized to access the file owner")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new FileCreation
										{
											Owner = EntityAddress.Parse<FairTable>(GetString(owner)), 
											Data = System.IO.File.ReadAllBytes(GetString(path)),
											Mime = GetEnum<FairMime>(mime, FairMime.None)
										};
							};
		return a;
	}
		
	public CommandAction Destroy_X()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Destroys existing file and all its associated data";
		a.Arguments =	[
							IdArgument("file to delete"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new FileDeletion {File = Id};
							};
		return a;
	}

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about the specified file";
		a.Arguments = [IdArgument("file to get information about")];	

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var	r = Ppc(new FilePpc(GetAutoId(IdKeyword))).File;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
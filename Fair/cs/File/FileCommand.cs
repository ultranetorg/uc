using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class FileCommand : FairCommand
{
	public FileCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string path = nameof(path);
		const string mime = nameof(mime);

		a.Name = "c";
		a.Description = "Creates a file entity";
		a.Arguments =	[
							new (null, EA, "Entity address of the file owner", ArgumentFlag.First),
							new (path, PATH, "Path to the file content"),
							new (mime, TEXT, "Mime type of the file content", ArgumentFlag.Optional),
							ByArgument("Name of the user authorized to access the file owner")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new FileCreation
										{
											Owner = EntityAddress.Parse<FairTable>(base.First), 
											Data = System.IO.File.ReadAllBytes(GetString(path)),
											Mime = GetEnum<FairMime>(mime, FairMime.None)
										};
							};
		return a;
	}
		
	public CommandAction Destroy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Description = "Destroys existing file and all its associated data";
		a.Arguments =	[
							new (null, EID, "Id of the file to delete", ArgumentFlag.First),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new FileDeletion {File = FirstAutoId};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Gets information about the specified file";
		a.Arguments = [new (null, EID, "Id of the file to get information about", ArgumentFlag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var	r = Ppc(new FilePpc(FirstAutoId)).File;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
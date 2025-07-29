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
					Syntax = $"{Keyword} {a.NamesSyntax} {EA} {path}={PATH} {SignerArg}={AA}",
					Arguments = [
									new ("<first>", "An entity address of file owner"),
									new (path, "A data associated with the file")
								],
					Examples =	[new (null, $"{Keyword} {a.Name} {EA.Example} {path}={PATH.Example} {SignerArg}={AA.Example}")]
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
					Syntax = $"{Keyword} {a.NamesSyntax} {EID} {SignerArg}={AA}",
					Arguments = [new ("<first>", "Id of a file to delete")],
					Examples = [new (null, $"{Keyword} {a.Name} {EID.Example} {SignerArg}={AA.Example}")]
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
					Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
					Arguments = [new ("<first>", "Id of a file to get information about")],
					Examples = [new (null, $"{Keyword} {a.Name} {SignerArg}={EID.Example}")]
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
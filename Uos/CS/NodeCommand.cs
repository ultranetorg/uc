using System.Reflection;

namespace Uccs.Uos;

//internal class NodeCommand : UosCommand
//{
//	public NodeCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
//	{
//	}
//
//	public CommandAction Run()
//	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
//
//		a.Name = "r";
//		a.Help = new() {Description = "Starts corresponding node if needed",
//						Syntax = $"{Keyword} {a.NamesSyntax} {NET} [custom start-up arguments]",
//
//						Arguments =	[
//										new (FirstArg, "A network address to connect to")
//									],
//
//						Examples =	[
//										new (null, $"{Keyword} {a.Name} {NET.Example}")
//									]};
//
//		a.Execute = () =>	{
//								Uos.ConnectNetwork(Args[0].Name);
//
//								return null;
//							};
//		
//		return a;
//	}
//}
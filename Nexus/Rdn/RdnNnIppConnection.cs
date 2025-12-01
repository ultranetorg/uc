using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Rdn;

//public interface RdnNnIpc<C, R> where C : Ipc<R> where R : IppResponse 
//{
	//public RdnNode Node => Owner as RdnNode;
//}

//public class RdnHolderClassesNnIpc : HolderClassesNnIpc
//{
//	public override IppResponse Execute(Flow flow)
//	{
//		return new HolderClassesNnIpr {Classes = [nameof(Account)]};
//	}
//}
//
//public class RdnHoldersByAccountNnIpc : HoldersByAccountNnIpc
//{
//	public RdnNode Node => Owner as RdnNode;
//
//	public override IppResponse Execute(Flow flow)
//	{
//		lock(Node.Mcv.Lock)
//		{	
//			var a = Node.Mcv.Accounts.Latest(new AccountAddress(Address));
//			
//			if(a != null)
//				return new HoldersByAccountNnIpr {Holders = [new AssetHolder {Class = nameof(Account), Id = a.Id.ToString()}]};
//			else
//				throw new NnException(NnError.NotFound);
//		}
//	}
//}

public class RdnNnIppConnection : NnpIppNodeConnection
{
	RdnNode Node => Program as RdnNode;

	public RdnNnIppConnection(RdnNode node, Flow flow) : base(node, NnpTcpPeering.GetName(node.NexusSettings.Host), flow)
	{
		RegisterHandler(typeof(NnClass), this);
	}

	public override void Established()
	{
		lock(Writer)
		{
			Writer.Write(NnpIppConnectionType.Node);
			Writer.WriteUtf8(Node.Net.Address);
		}
	}

	public Return HolderClasses(IppConnection connection, HolderClassesNna call)
	{
		return new HolderClassesNnr {Classes = [nameof(Account)]};
	}

//	public IppResponse AssetBalance(IppConnection connection, AssetBalanceNnIpc call)
//	{
//		throw new NotImplementedException();
//	}
//
//	public IppResponse AssetTransfer(IppConnection connection, AssetTransferNnIpc call)
//	{
//		throw new NotImplementedException();
//	}
//
//	public IppResponse HolderAssets(IppConnection connection, HolderAssetsNnIpc call)
//	{
//		throw new NotImplementedException();
//	}
//
//	public IppResponse HoldersByAccount(IppConnection connection, HoldersByAccountNnIpc call)
//	{
//		throw new NotImplementedException();
//	}
}

//public class HolderClassesNnIpc : NnIpc<HolderClassesNnIpr>
//{
//	public override CallReturn Execute()
//	{
//		return new HolderClassesNnIpr {Classes = [nameof(Account)]};
//	}
//}
//
//public class HolderClassesNnIpr : CallReturn
//{
//	public string[] Classes {get; set;}
//}
//
//public class HoldersByAccountNnIpc : NnIpc<HoldersByAccountNnIpr>
//{
//	public byte[]	Address { get; set; }
//
//	public override CallReturn Execute()
//	{
//		lock(Mcv.Lock)
//		{	
//			var a = Mcv.Accounts.Latest(new AccountAddress(Address));
//			
//			if(a != null)
//				return new HoldersByAccountNnIpr {Holders = [new AssetHolder {Class = nameof(Account), Id = a.Id.ToString()}]};
//			else
//				throw new NnException(NnError.NotFound);
//		}
//	}
//}
//
//public class HoldersByAccountNnIpr : CallReturn
//{
//	public AssetHolder[] Holders {get; set;}
//}
//
//public class HolderAssetsNnIpc : NnIpc<HolderAssetsNnIpr>
//{
//	public string	HolderClass { get; set; }
//	public string	HolderId { get; set; }
//
//	public override CallReturn Execute()
//	{
//		if(HolderClass != nameof(Account))
//			throw new NnException(NnError.Unknown);
//
//		lock(Mcv.Lock)
//		{	
//			var a = Mcv.Accounts.Latest(AutoId.Parse(HolderId));
//			
//			if(a != null)
//				return new HolderAssetsNnIpr{ Assets = [new () {Name = nameof(Account.Spacetime), Units = "Byte-days (BD)"},
//														new () {Name = nameof(Account.Energy), Units = "Execution Cycles (EC)"}]};
//			else
//				throw new NnException(NnError.NotFound);
//		}
//	}
//}
//
//public class HolderAssetsNnIpr : CallReturn
//{
//	public Asset[] Assets {get; set;}
//}
//
//public class AssetBalanceNnIpc : NnIpc<AssetBalanceNnIpr>
//{
//	public string	HolderClass { get; set; }
//	public string	HolderId { get; set; }
//	public string	Name { get; set; }
//
//	public override CallReturn Execute()
//	{
//		if(HolderClass != nameof(Account))
//			throw new NnException(NnError.Unknown);
//
//		if(Name != nameof(Account.Spacetime) && Name != nameof(Account.Energy))
//			throw new NnException(NnError.Unknown);
//
//		lock(Mcv.Lock)
//		{	
//			var a = Mcv.Accounts.Latest(AutoId.Parse(HolderId));
//			
//			if(a != null)
//				return	new AssetBalanceNnIpr
//						{
//							Balance = new BigInteger(Name switch
//														  {
//																nameof(Account.Spacetime) => a.Spacetime,
//																nameof(Account.Energy) => a.Energy,
//														  })
//						};
//			else
//				throw new NnException(NnError.NotFound);
//		}
//	}
//}
//
//public class AssetBalanceNnIpr : CallReturn
//{
//	public BigInteger Balance {get; set;}
//}
//
//public class AssetTransferNnIpc : NnIpc<AssetTransferNnIpr>
//{
//	public string	FromClass { get; set; }
//	public string	FromId { get; set; }
//	public string	ToNet { get; set; }
//	public string	ToClass { get; set; }
//	public string	ToId { get; set; }
//	public string	Name { get; set; }
//	public string	Amount { get; set; }
//
//	public override CallReturn Execute()
//	{
//		if(ToNet == Node.Net.Name)
//			throw new NnException(NnError.Unavailable);
//
//		return null;
//	}
//}
//
//public class AssetTransferNnIpr : CallReturn
//{
//}

using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Rdn;

public class RdnNnpIppConnection : McvNnpIppConnection<RdnNode, RdnTable>
{
	RdnNode Node => Program as RdnNode;

	public RdnNnpIppConnection(RdnNode node, Flow flow) : base(node, [nameof(Account)], flow)
	{
		RegisterHandler(typeof(NnpClass), this);
	}
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

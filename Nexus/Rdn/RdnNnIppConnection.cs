using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Rdn;

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
			Writer.WriteUtf8(Node.Settings.Api.LocalAddress(Node.Net));
		}
	}

	public Return HolderClasses(IppConnection connection, HolderClassesNna call)
	{
		return new HolderClassesNnr {Classes = [nameof(Account)]};
	}

	public Return AssetBalance(IppConnection connection, AssetBalanceNna call)
	{
		if(call.HolderClass != nameof(Account))
			throw new EntityException(EntityError.UnknownClass);

		if(call.Name != nameof(Account.Spacetime) && call.Name != nameof(Account.Energy) && call.Name != nameof(Account.EnergyNext))
			throw new EntityException(EntityError.UnknownAsset);

		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Accounts.Latest(AutoId.Parse(call.HolderId));
			
			if(a != null)
				return	new AssetBalanceNnr
						{
							Balance = new BigInteger(call.Name switch
															   {
																	nameof(Account.Spacetime) => a.Spacetime,
																	nameof(Account.Energy) => a.Energy,
															   })
						};
			else
				throw new EntityException(EntityError.NotFound);
		}
	}

	public Return HolderAssets(IppConnection connection, HolderAssetsNna call)
	{
		if(call.HolderClass != nameof(Account))
			throw new NnpException(NnError.Unknown);

		lock(Node.Mcv.Lock)
		{	
			if(!AutoId.TryParse(call.HolderId, out var id))
				throw new NnpException(NnError.NotFound);

			var a = Node.Mcv.Accounts.Latest(id);
			
			if(a != null)
				return new HolderAssetsNnr{Assets = [new () {Name = nameof(Account.Spacetime), Units = "Byte-days (BD)"},
													 new () {Name = nameof(Account.Energy), Units = "Execution Cycles (EC)"}]};
			else
				throw new EntityException(EntityError.NotFound);
		}
	}

	public Return HoldersByAccount(IppConnection connection, HoldersByAccountNna call)
	{
		lock(Node.Mcv.Lock)
		{	
			var a = Node.Mcv.Accounts.Latest(new AccountAddress(call.Address));
			
			if(a != null)
				return new HoldersByAccountNnr {Holders = [new AssetHolder {Class = nameof(Account), Id = a.Id.ToString()}]};
			else
				return new HoldersByAccountNnr {Holders = []};
		}
	}

	public Return AssetTransfer(IppConnection connection, AssetTransferNna call)
	{
		if(call.ToNet != Node.Net.Name)
			throw new NnpException(NnError.Unavailable);

		var t = new TransactApc
				{
					Signer = call.Signer, 
					Tag = Guid.CreateVersion7().ToByteArray(),
					Operations = [new UtilityTransfer
								 {
								 	FromTable	= (byte)Enum.Parse<RdnTable>(call.FromClass),
								 	From		= AutoId.Parse(call.FromId),
								 	ToTable		= (byte)Enum.Parse<RdnTable>(call.ToClass),
								 	To			= AutoId.Parse(call.ToId),
								 	Energy		= call.Name == nameof(Account.Energy) ? long.Parse(call.Amount) : 0, 
								 	EnergyNext	= call.Name == nameof(Account.EnergyNext) ? long.Parse(call.Amount) : 0,
								 	Spacetime	= call.Name == nameof(Account.Spacetime) ? long.Parse(call.Amount) : 0,
								 }] 
				};

		t.Execute(Node, null, null, Flow);

		var otc = new OutgoingTransactionApc {Tag = t.Tag};

		while((otc.Execute(Node, null, null, Flow) as TransactionApe).Status != TransactionStatus.Confirmed)
		{
			Thread.Sleep(10);
		}

		return new AssetTransferNnr {TransactionId = t.Tag};
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

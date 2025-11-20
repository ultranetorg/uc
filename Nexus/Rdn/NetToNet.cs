using System.Numerics;

namespace Uccs.Rdn;

public class HolderClassesNnc : Nnc<HolderClassesNnr>
{
	public override PeerResponse Execute()
	{
		return new HolderClassesNnr {Classes = [nameof(Account)]};
	}
}

public class HolderClassesNnr : PeerResponse
{
	public string[] Classes {get; set;}
}

public class HoldersByAccountNnc : Nnc<HoldersByAccountNnr>
{
	public byte[]	Address { get; set; }

	public override PeerResponse Execute()
	{
		lock(Mcv.Lock)
		{	
			var a = Mcv.Accounts.Latest(new AccountAddress(Address));
			
			if(a != null)
				return new HoldersByAccountNnr {Holders = [new AssetHolder {Class = nameof(Account), Id = a.Id.ToString()}]};
			else
				throw new NnException(NnError.NotFound);
		}
	}
}

public class HoldersByAccountNnr : PeerResponse
{
	public AssetHolder[] Holders {get; set;}
}

public class HolderAssetsNnc : Nnc<HolderAssetsNnr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }

	public override PeerResponse Execute()
	{
		if(HolderClass != nameof(Account))
			throw new NnException(NnError.Unknown);

		lock(Mcv.Lock)
		{	
			var a = Mcv.Accounts.Latest(AutoId.Parse(HolderId));
			
			if(a != null)
				return new HolderAssetsNnr{ Assets = [new () {Name = nameof(Account.Spacetime), Units = "Byte-days (BD)"},
														new () {Name = nameof(Account.Energy), Units = "Execution Cycles (EC)"}]};
			else
				throw new NnException(NnError.NotFound);
		}
	}
}

public class HolderAssetsNnr : PeerResponse
{
	public Asset[] Assets {get; set;}
}

public class AssetBalanceNnc : Nnc<AssetBalanceNnr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }
	public string	Name { get; set; }

	public override PeerResponse Execute()
	{
		if(HolderClass != nameof(Account))
			throw new NnException(NnError.Unknown);

		if(Name != nameof(Account.Spacetime) && Name != nameof(Account.Energy))
			throw new NnException(NnError.Unknown);

		lock(Mcv.Lock)
		{	
			var a = Mcv.Accounts.Latest(AutoId.Parse(HolderId));
			
			if(a != null)
				return	new AssetBalanceNnr
						{
							Balance = new BigInteger(Name switch
														  {
																nameof(Account.Spacetime) => a.Spacetime,
																nameof(Account.Energy) => a.Energy,
														  })
						};
			else
				throw new NnException(NnError.NotFound);
		}
	}
}

public class AssetBalanceNnr : PeerResponse
{
	public BigInteger Balance {get; set;}
}

public class AssetTransferNnc : Nnc<AssetTransferNnr>
{
	public string	FromClass { get; set; }
	public string	FromId { get; set; }
	public string	ToNet { get; set; }
	public string	ToClass { get; set; }
	public string	ToId { get; set; }
	public string	Name { get; set; }
	public string	Amount { get; set; }

	public override PeerResponse Execute()
	{
		if(ToNet == Node.Net.Name)
			throw new NnException(NnError.Unavailable);

		return null;
	}
}

public class AssetTransferNnr : PeerResponse
{
}

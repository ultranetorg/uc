using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Fair;

public class FairNnpIppConnection : McvNnpIppConnection<FairNode, FairTable>
{
	public FairNnpIppConnection(FairNode node, Flow flow) : base(node, [nameof(Account), nameof(Author), nameof(Site)], flow)
	{
	}

	public override Result AssetBalance(IppConnection connection, AssetBalanceNna call)
	{
		if(!Classes.Contains(call.HolderClass))
			throw new EntityException(EntityError.UnknownClass);

		if(!Assets.Any(i => i.Name == call.Name))
			throw new EntityException(EntityError.UnknownAsset);

		IHolder o = null;

		lock(Node.Mcv.Lock)
		{	
			switch(call.HolderClass)
			{
				case nameof(Account) :
				{
					o = Node.Mcv.Accounts.Latest(AutoId.Parse(call.HolderId));
					break;;
				}

				case nameof(Author) :
				{	
					o = Node.Mcv.Authors.Latest(AutoId.Parse(call.HolderId));
					break;;
				}

				case nameof(Site) :
				{	
					o = Node.Mcv.Sites.Latest(AutoId.Parse(call.HolderId));
					break;;
				}
			}

			if(o == null)
				throw new EntityException(EntityError.NotFound);

			return new AssetBalanceNnr{Balance = call.Name	switch
															{
																nameof(Account.Spacetime)	=> (o as ISpacetimeHolder).Spacetime,
																nameof(Account.Energy)		=> (o as IEnergyHolder).Energy,
																nameof(Account.EnergyNext)	=> (o as IEnergyHolder).EnergyNext,
																_							=> throw new EntityException(EntityError.UnknownAsset)
															}};
		}
	}
}

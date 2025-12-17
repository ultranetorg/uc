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
		if(!EntityAddress.TryParse<FairTable>(call.Entity, out var a))
			throw new EntityException(EntityError.UnknownClass);

		if(!Assets.Any(i => i.Name == call.Name))
			throw new EntityException(EntityError.UnknownAsset);

		IHolder o = null;

		lock(Node.Mcv.Lock)
		{	
			switch((FairTable)a.Table)
			{
				case FairTable.Account:
				{
					o = Node.Mcv.Accounts.Latest(a.Id);
					break;;
				}

				case FairTable.Author :
				{	
					o = Node.Mcv.Authors.Latest(a.Id);
					break;;
				}

				case FairTable.Site :
				{	
					o = Node.Mcv.Sites.Latest(a.Id);
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

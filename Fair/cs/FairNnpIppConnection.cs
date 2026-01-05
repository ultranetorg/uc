using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Fair;

public class FairNnpIppConnection : McvNnpIppConnection<FairNode, FairTable>
{
	public FairNnpIppConnection(FairNode node, Flow flow) : base(node, [nameof(User), nameof(Author), nameof(Site)], flow)
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
				case FairTable.User:
				{
					o = Node.Mcv.Users.Latest(a.Id);
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
																nameof(User.Spacetime)	=> (o as ISpacetimeHolder).Spacetime,
																nameof(User.Energy)		=> (o as IEnergyHolder).Energy,
																nameof(User.EnergyNext)	=> (o as IEnergyHolder).EnergyNext,
																_							=> throw new EntityException(EntityError.UnknownAsset)
															}};
		}
	}
}

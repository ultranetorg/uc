using System.Numerics;
using System.Text;
using Uccs.Net;

namespace Uccs.Fair;

public class FairNnpIppConnection : McvNnpIppConnection<FairNode, FairTable>
{
	public FairNnpIppConnection(FairNode node, Flow flow) : base(node, [nameof(Account), nameof(Author), nameof(Site)], flow)
	{
		RegisterHandler(typeof(NnpClass), this);
	}

	public override Result AssetBalance(IppConnection connection, AssetBalanceNna call)
	{
		if(!Classes.Contains(call.HolderClass))
			throw new EntityException(EntityError.UnknownClass);

		if(!Assets.Any(i => i.Name == call.Name))
			throw new EntityException(EntityError.UnknownAsset);

		long b = -1;

		lock(Node.Mcv.Lock)
		{	
			switch(call.HolderClass)
			{
				case nameof(Account) :
				{
					var a = Node.Mcv.Accounts.Latest(AutoId.Parse(call.HolderId));
			
					if(a == null)
						throw new EntityException(EntityError.NotFound);
					
					b = call.Name	switch
									{
										nameof(Account.Spacetime) => a.Spacetime,
										nameof(Account.Energy) => a.Energy,
										nameof(Account.EnergyNext) => a.EnergyNext,
									};
					break;;
				}

				case nameof(Author) :
				{	
					var a = Node.Mcv.Authors.Latest(AutoId.Parse(call.HolderId));
			
					if(a == null)
						throw new EntityException(EntityError.NotFound);
					
					b = call.Name	switch
									{
										nameof(Account.Spacetime) => a.Spacetime,
										nameof(Account.Energy) => a.Energy,
										nameof(Account.EnergyNext) => a.EnergyNext,
									};
					break;;
				}

				case nameof(Site) :
				{	
					var a = Node.Mcv.Sites.Latest(AutoId.Parse(call.HolderId));
			
					if(a == null)
						throw new EntityException(EntityError.NotFound);
					
					b = call.Name	switch
									{
										nameof(Account.Spacetime) => a.Spacetime,
										nameof(Account.Energy) => a.Energy,
										nameof(Account.EnergyNext) => a.EnergyNext,
									};
					break;;
				}
			}
		}

		return	new AssetBalanceNnr{Balance = b};
	}
}

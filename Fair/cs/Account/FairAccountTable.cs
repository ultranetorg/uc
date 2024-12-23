using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Fair;

public class FairAccountTable : AccountTable
{
	public FairAccountTable(Mcv chain) : base(chain)
	{
	}

	public override AccountEntry Create()
	{
		return new FairAccountEntry(Mcv);
	}
}

using System;
using System.IO;

namespace Uccs.Net
{
	public class BandwidthAllocation : Operation
	{
		public Unit				Bandwidth;
		public short			Days;
		public override string	Description => $"Allocation of {Bandwidth} EC for {Days} days";
		public override bool	IsValid(Mcv mcv) => Bandwidth >= 0 && Days > 0 && Days <= mcv.Zone.BandwidthAllocationDaysMaximum;
		
		public BandwidthAllocation()
		{
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Bandwidth	= reader.Read<Unit>();
			Days		= reader.ReadInt16();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Bandwidth);
			writer.Write(Days);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(Signer.BandwidthExpiration > round.ConsensusTime) /// refund 50% for what is left
			{
				var d = (Signer.BandwidthExpiration - round.ConsensusTime).Days;
				
				Signer.ECBalance += Signer.BandwidthNext * d / 2;

				for(int i=1; i<=d; i++)
					round.NextBandwidthAllocations[i] -= Signer.BandwidthNext;
			}

			for(int i=1; i<=Days; i++)
			{
				if(round.NextBandwidthAllocations[i] + Bandwidth < mcv.Zone.BandwidthAllocationPerDayMaximum)
				{
					round.NextBandwidthAllocations[i] += Bandwidth;
				}
				else
				{
					Error = LimitReached;
					return;
				}
			}

			Signer.BandwidthNext		= Bandwidth;
			Signer.BandwidthExpiration	= round.ConsensusTime + Time.FromDays(Days);
			
			Transaction.ECSpent		+= Bandwidth * Days;
			Transaction.ECReward	+= Bandwidth * Days;
		}
	}
}

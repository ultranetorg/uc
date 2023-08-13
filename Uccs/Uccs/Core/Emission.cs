using System.IO;
using System.Numerics;

namespace Uccs.Net
{
	public class Emission : Operation
	{
		public static readonly Coin		FactorStart = 0;
		public static readonly Coin		FactorEnd = 999;
		public static readonly Coin		FactorStep = new Coin(0.1);
		public static readonly Coin		Step = 1000;

		public BigInteger	Wei;
		public int			Eid;

		public override string Description => $"#{Eid} {Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Wei).ToString()} ETH -> {(Portion.Amount > 0 ? Portion.Amount : "???")} UNT";

		Portion Portion;

		public Emission() 
		{
		}

		public Emission(AccountAddress signer, BigInteger wei, int eid)
		{
			Signer = signer;
			Wei = wei;
			Eid = eid;
		}

		public override bool Valid => 0 < Wei && 0 <= Eid;

		protected override void ReadConfirmed(BinaryReader r)
		{
			Wei	= r.ReadBigInteger();
			Eid	= r.Read7BitEncodedInt();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.Write(Wei);
			w.Write7BitEncodedInt(Eid);
		}

		public override void Execute(Chainbase chain, Round round)
		{
			Portion = Calculate(round.WeiSpent, round.Factor, Wei);
			
			if(Portion.Factor <= FactorEnd)
			{
				round.AffectAccount(Signer).Balance += Portion.Amount;
				
				round.Fees += Portion.Amount / 10;
				round.Factor = Portion.Factor;
				round.WeiSpent += Wei;
				round.Emission += Portion.Amount;
			}
			else
				Error = "Emission is over"; /// emission is over
		}

		public static byte[] Serialize(AccountAddress beneficiary, int eid)
		{ 
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(beneficiary);
			w.Write7BitEncodedInt(eid);

			return s.ToArray();
		}
		
		public static Coin Multiplier(Coin factor)
		{
			return FactorEnd - factor;
		}

		public static Portion Calculate(BigInteger spent, Coin factor, BigInteger wei)
		{
			Coin f = factor;
			Coin a = 0;

			var d = Step - Coin.FromWei(spent) % Step;

			var w = Coin.FromWei(wei);

			if(w <= d)
			{
				a += w * Multiplier(f);
			}
			else
			{
				a += d * Multiplier(f);

				var r = w - d;

				while(f <= FactorEnd)
				{
					f = f + FactorStep;

					if(r < Step)
					{
						a += r * Multiplier(f);
						break;
					}
					else
					{
						a += Step * Multiplier(f);
						r -= Step;
					}
				}
			} 

			return new Portion { Factor = f, Amount = a};
		}
	}
}

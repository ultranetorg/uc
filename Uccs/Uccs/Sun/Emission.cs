using System.IO;
using System.Numerics;

namespace Uccs.Net
{
	public class Emission : Operation
	{
		public static readonly Coin		Multiplier = 1000;
		public static readonly Coin		End = new Coin(10_000_000_000);
		//public static readonly Coin		FactorStep = new Coin(0.1);
		//public static readonly Coin		Step = 1000;

		public BigInteger	Wei;
		public int			Eid;

		public override string Description => $"#{Eid}, {Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Wei)} ETH -> {Calculate(Wei)} UNT";

		Coin Portion;

		public Emission() 
		{
		}

		public Emission(BigInteger wei, int eid)
		{
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

 		public static Coin Calculate(BigInteger wei)
 		{
 			return Coin.FromWei(wei) * Multiplier;

// 		public static Portion Calculate(BigInteger spent, Coin factor, BigInteger wei)
// 		{
// 			Coin f = factor;
// 			Coin a = 0;
// 
// 			var d = Step - Coin.FromWei(spent) % Step;
// 
// 			var w = Coin.FromWei(wei);
// 
// 			if(w <= d)
// 			{
// 				a += w * Multiplier(f);
// 			}
// 			else
// 			{
// 				a += d * Multiplier(f);
// 
// 				var r = w - d;
// 
// 				while(f <= FactorEnd)
// 				{
// 					f = f + FactorStep;
// 
// 					if(r < Step)
// 					{
// 						a += r * Multiplier(f);
// 						break;
// 					}
// 					else
// 					{
// 						a += Step * Multiplier(f);
// 						r -= Step;
// 					}
// 				}
// 			} 
// 
// 			return new Portion { Factor = f, Amount = a};
 		}

		public override void Execute(Mcv mcv, Round round)
		{
			if(round.Emission > End)
			{
				Error = "Emission is over"; /// emission is over
				return;
			}

			Portion = Calculate(Wei);
			
			round.AffectAccount(Signer).Balance += Portion;
				
			round.Fees += Portion / 10;
			//round.Factor = Portion.Factor;
			//round.WeiSpent += Wei;
			round.Emission += Portion;
		}

		public static byte[] Serialize(AccountAddress beneficiary, int eid)
		{ 
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(beneficiary);
			w.Write7BitEncodedInt(eid);

			return s.ToArray();
		}
		
//		public static Coin Multiplier(Coin factor)
//		{
//			return FactorEnd - factor;
//		}
	}
}

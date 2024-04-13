using System.IO;
using System.Numerics;

namespace Uccs.Net
{
	public class Emission : Operation
	{
		public static readonly Money		Multiplier = 1000;
		public static readonly Money		End = new Money(10_000_000_000);

		public BigInteger	Wei;
		public int			Eid;

		public override string Description => $"#{Eid}, {Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Wei)} ETH -> {Calculate(Wei)} UNT";

		Money Portion;

		public Emission() 
		{
		}

		public Emission(BigInteger wei, int eid)
		{
			Wei = wei;
			Eid = eid;
		}

		public override bool Valid => 0 < Wei && 0 <= Eid;

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Wei);
			writer.Write7BitEncodedInt(Eid);
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Wei	= reader.ReadBigInteger();
			Eid	= reader.Read7BitEncodedInt();
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.Write(Wei);
			writer.Write7BitEncodedInt(Eid);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new Transaction();
			
			Transaction.Signer	= reader.ReadAccount();
			Wei					= reader.ReadBigInteger();
			Eid					= reader.Read7BitEncodedInt();
		}

 		public static Money Calculate(BigInteger wei)
 		{
 			return Money.FromWei(wei) * Multiplier;

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
		}

		public void ConsensusExecute(Round round)
		{
			if(round.Emission > End)
			{
				Error = "Emission is over"; /// emission is over
				return;
			}

			Portion = Calculate(Wei);
			
			var a = Affect(round, Signer);
			a.Balance += Portion;
			a.LastEmissionId = Eid;
				
			round.Emission += Portion;
			Reward += Portion / 10;
		}

		public static byte[] Serialize(AccountAddress beneficiary, int eid)
		{ 
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(beneficiary);
			w.Write7BitEncodedInt(eid);

			return s.ToArray();
		}
	}
}

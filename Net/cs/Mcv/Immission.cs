#if IMMISION

using System.IO;
using System.Numerics;

namespace Uccs.Net
{
	public class Immission : Operation
	{
		public static readonly Money		Multiplier = 1000;
		public static readonly Money		End = new Money(10_000_000_000);

		public BigInteger					Wei;
		public int							Eid;
		public Money						Portion;
		public Money						Fee;
		public EntityId						Generator;

		public override string Description => $"#{Eid}, {Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Wei)} ETH -> {Calculate(Wei)} UNT";

		public Immission() 
		{
		}

		public Immission(BigInteger wei, int eid)
		{
			Wei = wei;
			Eid = eid;
		}

		public override bool IsValid(Mcv mcv) => 0 < Wei && 0 <= Eid;

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
			writer.Write(Portion);
			writer.Write(Fee);
			writer.Write(Generator);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			Transaction = new Transaction();
			
			_Id					= reader.Read<OperationId>();
			Transaction.Signer	= reader.ReadAccount();
			Wei					= reader.ReadBigInteger();
			Eid					= reader.Read7BitEncodedInt();
			Portion				= reader.Read<Money>();
			Fee					= reader.Read<Money>();
			Generator			= reader.Read<EntityId>();
		}

 		public static Money Calculate(BigInteger wei)
 		{
 			return Money.FromWei(wei) * Multiplier;
 		}

		public override void Execute(Execution execution)
		{
			var a = mcv.Accounts.Find(Signer, round.Id);

			if(a.LastEmissionId != Eid - 1)
			{
				Error = "Wrond eid "; /// emission is over
				return;
			}

			Portion = Calculate(Wei);

			if(a.New)
			{
				Fee = round.AccountAllocationFee(a);

				if(Portion <= Fee)
				{ 
					Error = "Not enough emission to create account";
					return;
				}
			}

		}

		public void ConfirmedExecute(Round round)
		{
			if(round.Emission > End)
			{
				Error = "Emission is over"; /// emission is over
				return;
			}

			var a = Affect(round, Signer);
			a.Balance += Portion - Fee;
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

#endif
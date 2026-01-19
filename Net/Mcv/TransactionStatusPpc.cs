namespace Uccs.Net;

//public class TransactionsAddress : IBinarySerializable
//{
//	public AccountAddress	Signer { get; set; }
//	public int				Nid { get; set; }
//
//	public void Read(BinaryReader r)
//	{
//		Signer = r.ReadAccount();
//		Nid = r.Read7BitEncodedInt();
//	}
//
//	public void Write(BinaryWriter w)
//	{
//		w.Write(Signer); 
//		w.Write7BitEncodedInt(Nid);
//	}
//}

public class TransactionStatusPpc : McvPpc<TransactionStatusPpr>
{
	public byte[][]	Signatures { get; set; }

	public override Result Execute()
	{
		lock(Peering.Lock)
		{
			lock(Mcv.Lock)
			{
				RequireGraph();
	
				return	new TransactionStatusPpr
						{								
							LastConfirmedRoundId = Mcv.LastConfirmedRound.Id,
							Transactions = Signatures.Select(t =>	new TransactionStatusPpr.Item
																	{
																		Signature	= t,
																		Status		= (Peering.CandidateTransactions.Find(i => i.Signature.SequenceEqual(t))
																					  ?? 
																					  Peering.ConfirmedTransactions.Find(i => i.Signature.SequenceEqual(t)))?.Status ?? TransactionStatus.FailedOrNotFound
																	})
														.ToArray()
						};
			}
		}
	}
}

public class TransactionStatusPpr : Result
{
	public class Item
	{
		public byte[]				Signature { get; set; }
		public TransactionStatus	Status { get; set; }
	}

	public int		LastConfirmedRoundId { get; set; }
	public Item[]	Transactions { get; set; }
}

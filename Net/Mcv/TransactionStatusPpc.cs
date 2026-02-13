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
	
				var r = new TransactionStatusPpr
						{								
							Transactions = Signatures.Select(s =>	{ 
																		var  t = (Peering.CandidateTransactions.Find(i => i.Signature.SequenceEqual(s))
																				 ?? 
																				 Peering.ConfirmedTransactions.Find(i => i.Signature.SequenceEqual(s)));

																		if(t != null)
																			t.Inquired = DateTime.UtcNow;

																		return	new TransactionStatusPpr.Item
																				{
																					Signature	= s,
																					Status		= t?.Status ?? TransactionStatus.FailedOrNotFound
																				};
																	})
														.ToArray()
						};

				return r;
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

	public Item[]	Transactions { get; set; }
}

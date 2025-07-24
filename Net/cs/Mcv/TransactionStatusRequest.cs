namespace Uccs.Net;

public class TransactionsAddress : IBinarySerializable
{
	public AccountAddress	Signer { get; set; }
	public int				Nid { get; set; }

	public void Read(BinaryReader r)
	{
		Signer = r.ReadAccount();
		Nid = r.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter w)
	{
		w.Write(Signer); 
		w.Write7BitEncodedInt(Nid);
	}
}

public class TransactionStatusRequest : McvPpc<TransactionStatusResponse>
{
	public TransactionsAddress[]	Transactions { get; set; }

	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			lock(Mcv.Lock)
			{
				RequireGraph();
	
				return	new TransactionStatusResponse
						{								
							LastConfirmedRoundId = Mcv.LastConfirmedRound.Id,
							Transactions = Transactions.Select(t => new{Q = t,
																		T = Peering.IncomingTransactions.Find(i => i.Signer == t.Signer && i.Nid == t.Nid)
																			?? 
																			//Peering.OutgoingTransactions.Find(i => i.Signer == t.Signer && i.Nid == t.Nid)
																			//Mcv.FindTailTransaction(i => i.Signer == t.Signer && i.Nid == t.Nid)
																			Peering.ArchivedTransactions.Find(i => i.Signer == t.Signer && i.Nid == t.Nid)
																			})
														.Select(i => new TransactionStatusResponse.Item{Account	= i.Q.Signer,
																										Id		= i.T?.Id ?? default,
																										Nid		= i.Q.Nid,
																										Status	= i.T == null ? TransactionStatus.FailedOrNotFound : i.T.Status})
														.ToArray()
						};
			}
		}
	}
}

public class TransactionStatusResponse : PeerResponse
{
	public class Item
	{
		public AccountAddress		Account { get; set; }
		public int					Nid { get; set; }
		public TransactionId		Id  { get; set; }
		public TransactionStatus	Status { get; set; }
	}

	public int		LastConfirmedRoundId { get; set; }
	public Item[]	Transactions { get; set; }
}

namespace Uccs.Net
{
	public class TransactionsAddress : IBinarySerializable
	{
		public AccountAddress	Account { get; set; }
		public int				Nid { get; set; }

		public void Read(BinaryReader r)
		{
			Account = r.ReadAccount();
			Nid = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Account); 
			w.Write7BitEncodedInt(Nid);
		}
	}

	public class TransactionStatusRequest : McvCall<TransactionStatusResponse>
	{
		public TransactionsAddress[]	Transactions { get; set; }

		public override PeerResponse Execute()
		{
			lock(Peering.Lock)
			{
				lock(Mcv.Lock)
				{
					RequireBase();
		
					return new TransactionStatusResponse
							{
								LastConfirmedRoundId = Mcv.LastConfirmedRound.Id,
								Transactions = Transactions.Select(t => new{Q = t,
																			T = Peering.IncomingTransactions.Find(i => i.Signer == t.Account && i.Nid == t.Nid)
																				?? 
																				Mcv.Accounts.FindLastTransaction(t.Account, i => i.Nid == t.Nid)})
															.Select(i => new TransactionStatusResponse.Item{Account	= i.Q.Account,
																											Id		= i.T == null ? default : i.T.Id,
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
}

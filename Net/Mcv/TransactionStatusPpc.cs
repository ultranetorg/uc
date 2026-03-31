namespace Uccs.Net;

public class TransactionStatusPpc : McvPpc<TransactionStatusPpr>
{
	public byte[][]	Tags { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();
		
		lock(Mcv.Lock)
		{
			var r = new TransactionStatusPpr
					{								
						Transactions = Tags.Select(s =>	{ 
															var t = Peering.CandidateTransactions.Find(i => i.Tag.SequenceEqual(s))
																	?? 
																	Peering.ConfirmedTransactions.Find(i => i.Tag.SequenceEqual(s));

															if(t != null)
																t.Inquired = DateTime.UtcNow;

															return	new TransactionStatusPpr.Item
																	{
																		Tag		= s,
																		Status	= t?.Status ?? TransactionStatus.FailedOrNotFound,
																		Error	= t?.OverallError
																	};
														})
											.ToArray()
					};

			return r;
		}
	}
}

public class TransactionStatusPpr : Result
{
	public class Item
	{
		public byte[]				Tag { get; set; }
		public string				Error { get; set; }
		public TransactionStatus	Status { get; set; }
	}

	public Item[]	Transactions { get; set; }
}

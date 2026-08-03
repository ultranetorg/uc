namespace Uccs.Net;

public class TransactionStatusPpc : McvPpc<TransactionStatusPpr>
{
	public byte[][]	Tags { get; set; }

	public override Result Execute()
	{
		lock(Mcv.Lock)
			RequireMember();

		var r = new TransactionStatusPpr
				{								
					Transactions = [..Tags.Select(s =>	{ 
															Transaction t = null;
														
															lock(Peering.CandidateTransactions)
																t = Peering.CandidateTransactions.Find(i => i.Tag.SequenceEqual(s));
														
															if(t == null)
																lock(Peering.ConfirmedTransactions)
																{	
																	t = Peering.ConfirmedTransactions.Find(i => i.Tag.SequenceEqual(s));

																	if(t != null)
																		t.Inquired = DateTime.UtcNow;
																}

															return	new TransactionStatusPpr.Item
																	{
																		Tag		= s,
																		Status	= t?.Status ?? TransactionStatus.FailedOrNotFound,
																		Error	= t?.OverallError
																	};
														})]
				};

		return r;
	}

	public override void Read(Reader reader)
	{
		Tags = reader.ReadArray(reader.ReadBytes);
	}

	public override void Write(Writer writer)
	{
		writer.Write(Tags, writer.WriteBytes);
	}

}

public class TransactionStatusPpr : Result
{
	public class Item
	{
		public byte[]				Tag { get; set; }
		public TransactionStatus	Status { get; set; }
		public string				Error { get; set; }
	}

	public Item[]	Transactions { get; set; }

	public override void Write(Writer writer)
	{
		writer.Write(Transactions, i =>	{
											writer.WriteBytes(i.Tag);
											writer.Write(i.Status);
											writer.WriteASCII(i.Error);
										});
	}

	public override void Read(Reader reader)
	{
		Transactions = reader.ReadArray(() =>	new Item
												{
													Tag = reader.ReadBytes(), 
													Status = reader.Read<TransactionStatus>(), 
													Error = reader.ReadASCII(), 
												});
	}

}

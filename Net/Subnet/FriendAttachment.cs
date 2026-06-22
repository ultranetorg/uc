namespace Uccs.Net;

public class FriendAttachment : Operation, IOutwardOperation
{
	public string				Name  { get; set; }
	public Snq					Client  { get; set; }
	public Endpoint[]			Peers { get; set; }

	public override string		Explanation => $"Name={Name}, Client={Client}, Peers={{{Peers.Length}}}";
	
	public FriendAttachment()
	{
	}
		
	public override bool IsValid(McvNet net)
	{ 
		if(!Friend.Valid(Name))
			return false;

		return true;
	}

	public override void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Client	= reader.Read<Snq>();
		Peers	= reader.ReadArray<Endpoint>();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Client);
		writer.Write(Peers);
	}

	public override void Execute(Execution execution)
	{
		if(execution.Friends.Find(Name) != null)
		{
			Error = AlreadyExists;
			return;
		}

		execution.AffectOutwards();
		execution.OutwardTransactions.Add(	new OutwardTransaction
											{
												Id			= ++User.LastOutward,
												User		= User.Id, 
												//Generator	= Transaction.Vote.Member,  
												Operation	= this,
												Expiration	= execution.Time + execution.Net.OutwardVerificationDurationLimit
											});

	
		execution.PayOperationEnergy(User);
	}

	public void SuccessExecute(Execution execution, OutwardTransaction task)
	{
		var s = execution.Friends.Affect(Name);

		s.Peers					= Peers;
		s.Client				= Client;
		s.OutStatus				= IccTransferStatus.None;
		s.LastIncomingTransfer	= new() {Hash = execution.Net.Cryptography.ZeroHash, Results = []};
		s.LastOutgoingTransfer	= new() {Transactions = []};
		s.Balances				= [];
	}
}

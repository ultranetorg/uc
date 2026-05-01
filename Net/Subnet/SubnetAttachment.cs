namespace Uccs.Net;

public class SubnetAttachment : OutwardOperation
{
	public string				Name  { get; set; }
	public Snp					Client  { get; set; }
	public Endpoint[]			Peers { get; set; }

	public override string		Explanation => $"Name={Name}, Client={Client}, Peers={{{Peers.Length}}}";
	
	public SubnetAttachment()
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
		Client	= reader.Read<Snp>();
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
		execution.OutwardTransactions.Add(	new OutwardTransaction(execution.Net)
											{
												Id			= ++User.LastOutward,
												User		= User.Id, 
												Generator	= Transaction.Member,  
												Operation	= this,
												Expiration	= execution.Time + execution.Net.ForeignVerificationDurationLimit
											});

	
		execution.PayOperationEnergy(User);

//		if(RequireSignerDomain(execution, Name, out var e) == false)
//			return;
//
// 		if(e.NnChildNet != null)
// 		{
// 			Error = AlreadyExists;
// 			return;
// 		}
// 
// 		if(!Uccs.Rdn.Domain.IsRoot(e.Address))
// 		{
// 			Error = NotRoot;
// 			return;
// 		}
// 
// 		e = execution.Domains.Affect(Domain);
// 
//  		e.NnChildNet = Net;
// 	
// 		execution.PayOperationEnergy(User);
	}

	public override void ConfirmedExecute(Execution execution, OutwardTransaction task)
	{
		var s = execution.Friends.Affect(Name);

		s.Peers		= Peers;
		s.Client	= Client;
		s.OutStatus	= IccTransferStatus.None;
	}
}

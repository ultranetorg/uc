namespace Uccs.Rdn;

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
		if(!Subnet.Valid(Name))
			return false;

		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Name	= reader.ReadASCII();
		Client	= reader.Read<Snp>();
		Peers	= reader.ReadArray<Endpoint>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Name);
		writer.Write(Client);
		writer.Write(Peers);
	}

	public override void Execute(RdnExecution execution)
	{
		if(execution.Subnets.Find(Name) != null)
		{
			Error = AlreadyExists;
			return;
		}

		execution.AffectOutwards();
		execution.Outwards.Add(	new Outward(execution.Net)
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

	public override void ConfirmedExecute(RdnExecution execution, Outward task)
	{
		var s = execution.Subnets.Affect(Name);

		s.Peers		= Peers;
		s.Client	= Client;
		s.OutHash	= execution.Net.Cryptography.ZeroHash;
		s.OutStatus	= OutTransactionStatus.None;
	}
}

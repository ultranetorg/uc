namespace Uccs.Fair;

public class AuthorVerification : FairOperation, IOutwardOperation
{ 
	public AutoId			Author  { get; set; }
	public string			Webdomain  { get; set; }

	public override string	Explanation => $"{Author}.{Webdomain}";

	public AuthorVerification()
	{
	}

	public AuthorVerification(AutoId author, string domain)
	{
		Author = author;
		Webdomain = domain;
	}

	public override bool IsValid(McvNet net)
	{
		if(Uri.CheckHostName(Webdomain) != System.UriHostNameType.Dns)
			return false;

		return true;
	}
	
	public override void Read(Reader reader)
	{
		Author		= reader.Read<AutoId>();
		Webdomain	= reader.ReadUtf8();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
		writer.WriteUtf8(Webdomain);
	}

	public override void Execute(FairExecution execution)
	{
		if(execution.OutwardTransactions.Count >= McvNet.OutwardsMaximum)
		{
			Error = LimitExceeded;
			return;
		}

		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		a = execution.Authors.Find(a.Id);

		execution.AffectOutwards();
		execution.OutwardTransactions.Add(	new OutwardTransaction
											{
												Id			= ++User.LastOutward,
												User		= User.Id, 
												Operation	= this,
												Expiration	= execution.Time + execution.Net.OutwardVerificationDurationLimit
											});

	
		execution.PayOperationEnergy(User);
		execution.PayEnergy(User, execution.Net.OutwardVerificationEnergyCost);
	}

	public void SuccessExecute(Execution execution, OutwardTransaction task)
	{
		var e = execution as FairExecution;
		var a = e.Authors.Affect(Author);

		a.VerifiedWebdomain		= Webdomain;
		a.VerifiedWebdomainRank	= 1_000_000_000 / AuthorExecution.GetRank(e, Webdomain);

		foreach(var p in a.Products.Select(i => e.Products.Find(i)))
		{
			foreach(var b in p.Publications.Select(i => e.Publications.Affect(i)))
			{
				b.AuthorRank = a.VerifiedWebdomainRank;
			}
		}
	}
}

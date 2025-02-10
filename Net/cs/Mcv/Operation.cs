namespace Uccs.Net;

public struct Portion
{
	public Unit	Factor;
	public Unit	Amount;
}

public enum OperationClass
{
	None = 0, 
	CandidacyDeclaration		= 000_000_001, 
	UtilityTransfer				= 000_000_002,
	BandwidthAllocation			= 000_000_003,

	ChildNet					= 001, 
		ChildNetInitialization	= 001_000_001,
}


public abstract class Operation : ITypeCode, IBinarySerializable
{
	public string			Error;
	public Transaction		Transaction;
	public AccountEntry		Signer;
	public abstract string	Description { get; }
	public long				ECExecuted;

	public const string		Rejected = "Rejected";
	public const string		NotFound = "Not found";
	public const string		NotAvailable = "Not Available";
	public const string		Mismatch = "Mismatch";
	public const string		ExistingAccountRequired = "ExistingAccountRequired";
	public const string		Expired = "Expired";
	public const string		Sealed = "Sealed";
	public const string		NotSealed = "NotSealed";
	public const string		NoData = "NoData";
	public const string		AlreadyExists = "Already exists";
	public const string		NotSequential = "Not sequential";
	public const string		NotEnoughBD = "Not enough spacetime";
	public const string		NotEnoughEC = "Not enough execution units";
	public const string		NotEnoughMR = "Not enough membership rights";
	public const string		NotEnoughBandwidth = "Not enough bandwidth";
	public const string		NoAnalyzers = "No analyzers";
	public const string		Denied = "Access denied";
	public const string		NotRelease = "Data valus is not a release";
	public const string		LimitReached = "Limit Reached";

	protected OperationId	_Id;
	
	public OperationId Id
	{
		get
		{
			if(_Id == default)
			{
				_Id = new (Transaction.Id.Ri, Transaction.Id.Ti, (byte)Array.IndexOf(Transaction.Operations, this));
			}

			return _Id;
		}
	}
	
	static Operation()
	{
	}

	public Operation()
	{
	}
	
	public abstract bool IsValid(Mcv mcv);
	public abstract void Execute(Mcv mcv, Round round);
	public abstract void WriteConfirmed(BinaryWriter w);
	public abstract void ReadConfirmed(BinaryReader r);
	 
	public override string ToString()
	{
		return $"{GetType().Name}, {Description}{(Error == null ? null : ", Error=" + Error)}";
	}

	public void Read(BinaryReader reader)
	{
		ReadConfirmed(reader);
	}

	public void Write(BinaryWriter writer)
	{
		WriteConfirmed(writer);
	}

	public AccountEntry Affect(Round round, AccountAddress account)
	{
		var e = round.AffectAccount(account);	

		if(e.New && (Signer.Address != round.Mcv.Net.God || round.Id > Mcv.LastGenesisRound)) /// new Account
		{
			Signer.BDBalance -= round.AccountAllocationFee(e);
		}

		return e;
	}

	public AccountEntry RequireAccount(Round round, AccountAddress account)
	{
		var a = round.Mcv.Accounts.Find(account, round.Id);

		if(a == null)
		{
			Error = NotFound;
			return a;
		}

		return a;
	}

	public bool Pay(ref EC[] from, ref EC[] to, long y, Time time)
	{
		if(EC.Integrate(from, time) < y)
		{
			Error = NotEnoughEC;
			return false;
		}

		EC.Move(ref from, ref to, y, time);

		return true;
	}
}

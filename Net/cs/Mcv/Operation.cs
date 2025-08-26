namespace Uccs.Net;

public struct Portion
{
	public Unit	Factor;
	public Unit	Amount;
}

public enum OperationClass
{
	None = 0, 
	Genesis						= 000_000_001, 
	AccountCreation				= 000_000_002, 
	CandidacyDeclaration		= 000_000_003, 
	UtilityTransfer				= 000_000_004,
	BandwidthAllocation			= 000_000_005,

	ChildNet					= 001, 
		ChildNetInitialization	= 001_000_001,
}


public abstract class Operation : ITypeCode, IBinarySerializable
{
	public string						Error;
	public Transaction					Transaction;
	public Account						Signer;
	public abstract string				Explanation { get; }

	public const string					AlreadyExists = "Already exists";
	public const string					AtLeastOneOwnerRequired = "At least one owner required";
	public const string					Denied = "Access denied";
	public const string					ExistingAccountRequired = "ExistingAccountRequired";
	public const string					Expired = "Expired";
	public const string					LimitReached = "Limit Reached";
	public const string					OutOfBounds = "Out Of Bounds";
	public const string					Mismatch = "Mismatch";
	public const string					NotAvailable = "Not Available";
	public const string					NotFound = "Not found";
	public const string					NotSequential = "Not sequential";
	public const string					NotEnergyHolder = "Not Energy Holder";
	public const string					NotEnoughSpacetime = "Not enough spacetime";
	public const string					NotEnoughEnergy = "Not enough execution units";
	public const string					NotEnoughEnergyNext = "Not enough energy for next period";
	public const string					NotEnoughBandwidth = "Not enough bandwidth";
	public const string					NotSpacetimeHolder = "Not spacetime holder";
	public const string					NothingLastCreated = "Nothing last created";
	public const string					Rejected = "Rejected";

	protected OperationId				_Id;
	
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

	public Operation()
	{
	}
	
	public abstract bool IsValid(McvNet net);
	public abstract void Execute(Execution execution);
	public abstract void Write(BinaryWriter w);
	public abstract void Read(BinaryReader r);
	 
	public override string ToString()
	{
		return $"{GetType().Name}, {Explanation}{(Error == null ? null : ", Error=" + Error)}";
	}
	
	public virtual void PreTransact(McvNode node, bool sponsored, Flow flow)
	{
	}

	public bool AccountExists(Execution executions, AutoId id, out Account account, out string error)
	{
		account = executions.FindAccount(id);

		if(account == null || account.Deleted)
		{
			error = NotFound;
			return false;
		}

		error = null;
		return true;
	}

	public bool CanAccessAccount(Execution executions, AutoId id, out Account account, out string error)
	{
		if(!AccountExists(executions, id, out account, out error))
			return false;

		if(account.Address != Signer.Address)
		{
			error = Denied;
			return false;
		}

		return true;
	}
}

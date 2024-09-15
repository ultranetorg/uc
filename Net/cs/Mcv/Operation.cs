namespace Uccs.Net
{
	public struct Portion
	{
		public Unit	Factor;
		public Unit	Amount;
	}

	public enum OperationClass
	{
		None = 0, 
		CandidacyDeclaration, 
		//Immission,
		UnitTransfer,
		BandwidthAllocation
	}

	public abstract class Operation : ITypeCode, IBinarySerializable
	{
		public string			Error;
		public Transaction		Transaction;
		public AccountEntry		Signer;
		public abstract string	Description { get; }

		public const string		Rejected = "Rejected";
		public const string		NotFound = "Not found";
		public const string		NotAvailable = "Not Available";
		public const string		NotPermitted = "Not found";
		public const string		ExistingAccountRequired = "ExistingAccountRequired";
		public const string		Expired = "Expired";
		public const string		Sealed = "Sealed";
		public const string		NotSealed = "NotSealed";
		public const string		NoData = "NoData";
		public const string		AlreadyExists = "Already exists";
		public const string		NotSequential = "Not sequential";
		public const string		NotEnoughBY = "Not enough spacetime";
		public const string		NotEnoughEC = "Not enough execution units";
		public const string		NotEnoughMR = "Not enough membership rights";
		public const string		NoAnalyzers = "No analyzers";
		public const string		NotOwner = "The signer does not own the entity";
		public const string		CantChangeSealedResource = "Cant change sealed resource";
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

			if(e.New && (Signer.Address != round.Mcv.Zone.God || round.Id > Mcv.LastGenesisRound)) /// new Account
			{
				Signer.BYBalance -= round.AccountAllocationFee(e);
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
	}
}

﻿using System;
using System.IO;

namespace Uccs.Net
{
	public struct Portion
	{
		public Money	Factor;
		public Money	Amount;
	}

	public enum OperationClass
	{
		None = 0, 
		CandidacyDeclaration, 
		Immission,
		UntTransfer
	}

	public abstract class Operation : ITypeCode, IBinarySerializable
	{
		public string			Error;
		public int				ExeUnits;
		public Money			Reward;
		public Transaction		Transaction;
		public AccountAddress	Signer => Transaction.Signer;
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
		public const string		NotEnoughUNT = "Not enough UNT";
		public const string		NoAnalyzers = "No analyzers";
		public const string		NotOwner = "The signer does not own the entity";
		public const string		CantChangeSealedResource = "Cant change sealed resource";
		public const string		NotRelease = "Data valus is not a release";

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

			if(e.New) /// new Account
			{
				round.AffectAccount(Signer).Balance -= round.AccountAllocationFee(e);
			}

			return e;
		}
	}

}
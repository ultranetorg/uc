using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Uccs.Net
{
	public enum PlacingStage
	{
		Null, 
		Pending,
		Accepted, /*Verified, */Placed, FailedOrNotFound, Confirmed
	}

	public struct Portion
	{
		public Money	Factor;
		public Money	Amount;
	}

	public enum OperationClass
	{
		Null = 0, 
		CandidacyDeclaration, 
		Emission, UntTransfer, 
		AuthorBid, AuthorRegistration, AuthorTransfer, ResourceCreation, ResourceUpdation,
	}

	public abstract class Operation// : ITypedBinarySerializable
	{
		public string			Error;
		public AccountAddress	Signer { get; set; }
		public Transaction		Transaction;
		//public Workflow		FlowReport;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}

		public const string		Rejected = "Rejected";
		public const string		NotFound = "Not found";
		public const string		AlreadyExists = "Alreadt exists";
		public const string		NotSequential = "Not sequential";
		public const string		NotEnoughUNT = "Not enough UNT";
		public const string		NotOwner = "The signer does not own the entity";
		public const string		CantChangeSealedResource = "Cant change sealed resource";

		public OperationClass	Class => Enum.Parse<OperationClass>(GetType().Name);

		protected OperationId	_id;
		
		public OperationId Id
		{
			get
			{
				if(_id == default)
				{
					_id = new (Transaction.Id.Ri, Transaction.Id.Ti, (byte)Array.IndexOf(Transaction.Operations, this));
				}

				return _id;
			}
		}
			

		public Operation()
		{
		}
		
		public abstract void Execute(Mcv chain, Round round);
		public abstract void WriteConfirmed(BinaryWriter w);
		public abstract void ReadConfirmed(BinaryReader r);

		public static Operation FromType(OperationClass type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(Operation).Namespace + "." + type).GetConstructor(new System.Type[]{}).Invoke(new object[]{}) as Operation;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Operation)} type", ex);
			}
		}
		 
		public override string ToString()
		{
			return $"{Class}, {Description}{(Error == null ? null : ", Error=" + Error)}";
		}

		public void Read(BinaryReader reader)
		{
			ReadConfirmed(reader);
		}

		public void Write(BinaryWriter writer)
		{
			WriteConfirmed(writer);
		}

		public int CalculateSize()
		{
			var s = new FakeStream();
			var w = new BinaryWriter(s);

			WriteConfirmed(w);

			return (int)s.Length;
		}

		public Money CalculateTransactionFee(Money feeperbyte)
		{
			int size = CalculateSize();

			return feeperbyte * size;
		}

		public void PayForAllocation(int extralength, byte years)
		{
			var fee = Mcv.CalculateSpaceFee(Mcv.EntityAllocationBaseLength + extralength, years);
			
			AffectAccount(Signer).Balance -= fee;
			Transaction.Round.Fees += fee;
		}

		public AccountEntry AffectAccount(AccountAddress account)
		{
			var r = Transaction.Round;
			var e = r.Mcv.Accounts.Find(account, r.Id);	

			if(e == null) /// new account
			{
				var	s = r.AffectAccount(Signer);
				
				s.Balance -= Mcv.AccountAllocationFee;
				r.Fees += Mcv.AccountAllocationFee;
			}

			return r.AffectAccount(account);
		}

		public AuthorEntry AffectAuthor(string author)
		{
			return Transaction.Round.AffectAuthor(author);
		}
	}
}

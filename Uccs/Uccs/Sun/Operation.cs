using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Nethereum.Hex.HexConvertors.Extensions;

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
		Emission, UntTransfer, 
		AuthorBid, AuthorRegistration, AuthorTransfer,
		ResourceCreation, ResourceUpdation,
		AnalysisOrder, AnalysisRegistration
	}

	public abstract class Operation// : ITypedBinarySerializable
	{
		public string			Error;
		//public AccountAddress	Signer { get; set; }
		public Transaction		Transaction;
		public AccountAddress	Signer => Transaction.Signer;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}

		public const string		Rejected = "Rejected";
		public const string		NotFound = "Not found";
		public const string		AlreadyExists = "Already exists";
		public const string		NotSequential = "Not sequential";
		public const string		NotEnoughUNT = "Not enough UNT";
		public const string		NoAnalyzers = "No analyzers";
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
		
		public abstract void Execute(Mcv mcv, Round round);
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

		public static Money CalculateSpaceFee(int size, byte years)
		{
			return Mcv.SpaceBasicFeePerByte * size * new Money(1u << (years - 1));
		}

		public void Pay(Round round, int length, byte years)
		{
			var fee = CalculateSpaceFee(length, years);
			
			Affect(round, Signer).Balance -= fee;
			round.Fees += fee;
		}

		public AccountEntry Affect(Round round, AccountAddress account)
		{
			var e = round.Mcv.Accounts.Find(account, round.Id);	

			if(e == null) /// new account
			{
				Pay(round, Mcv.EntityAllocationAverageLength, 15);
			}

			return round.AffectAccount(account);
		}

		public AuthorEntry Affect(Round round, string author)
		{
			return round.AffectAuthor(author);
		}

		public AnalysisEntry Affect(Round round, byte[] release)
		{
			var e = round.Mcv.Analyses.Find(release, round.Id);	

			if(e == null) /// new account
			{
				Pay(round, Mcv.EntityAllocationAverageLength, 1);
			}

			return round.AffectAnalysis(release);
		}
	}

	public class OperationJsonConverter : JsonConverter<Operation>
	{
		public override Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString().Split(':');
			var o = Operation.FromType(Enum.Parse<OperationClass>(s[0]));
 			
			o.Read(new BinaryReader(new MemoryStream(s[1].HexToByteArray()))); 

			return o;
		}

		public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			value.Write(w);
			
			writer.WriteStringValue(value.Class.ToString() + ":" + s.ToArray().ToHex());
		}
	}

}

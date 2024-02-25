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
		AnalysisRegistration, AnalysisResultRegistration
	}

	public abstract class Operation : ITypeCode, IBinarySerializable
	{
		public string			Error;
		public Money			Fee;
		//public AccountAddress	Signer { get; set; }
		public Transaction		Transaction;
		public AccountAddress	Signer => Transaction.Signer;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}

		public const string		Rejected = "Rejected";
		public const string		NotFound = "Not found";
		public const string		Expired = "Expired";
		public const string		Sealed = "Sealed";
		public const string		AlreadyExists = "Already exists";
		public const string		NotSequential = "Not sequential";
		public const string		NotEnoughUNT = "Not enough UNT";
		public const string		NoAnalyzers = "No analyzers";
		public const string		NotOwner = "The signer does not own the entity";
		public const string		CantChangeSealedResource = "Cant change sealed resource";
		public const string		NotRelease = "Data valus is not a release";

		public OperationClass	Class => Enum.Parse<OperationClass>(GetType().Name);
		public byte				TypeCode => (byte)Class;

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
			

		public Operation()
		{
		}
		
		public abstract void Execute(Mcv mcv, Round round);
		public abstract void WriteConfirmed(BinaryWriter w);
		public abstract void ReadConfirmed(BinaryReader r);

		public static Operation FromType(OperationClass type)
		{
			return Assembly.GetExecutingAssembly().GetType(typeof(Operation).Namespace + "." + type).GetConstructor(new System.Type[]{}).Invoke(null) as Operation;
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

		public static Money CalculateEntityFee(Money rentperentity, byte years)
		{
			return rentperentity * new Money(Mcv.RentFactor(years));
		}

		public static Money CalculateResourceDataFee(Money rentperbyte, int size, byte years)
		{
			return rentperbyte * size * new Money(Mcv.RentFactor(years));
		}

		public void PayForBytes(Round round, int length, byte years)
		{
			var fee = CalculateResourceDataFee(round.RentalPerByte, length, years);
			
			Affect(round, Signer).Balance -= fee;
			Fee += fee;
		}

		public void PayForEntity(Round round, byte years)
		{
			var fee = CalculateEntityFee(round.RentPerEntity, years);
			
			Affect(round, Signer).Balance -= fee;
			Fee += fee;
		}

		public AccountEntry Affect(Round round, AccountAddress account)
		{
			var e = round.Mcv.Accounts.Find(account, round.Id);	

			if(e == null) /// new Account
			{
				PayForEntity(round, 1);
			}

			return round.AffectAccount(account);
		}

		public AuthorEntry Affect(Round round, string author)
		{
			return round.AffectAuthor(author);
		}

		public ReleaseEntry Affect(Round round, ReleaseAddress release)
		{
			var e = round.Mcv.Releases.Find(release, round.Id);	

			if(e == null) /// new Analysis
			{
				PayForEntity(round, 1);
			}

			return round.AffectRelease(release);
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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
		DomainMigration, DomainBid, DomainRegistration, DomainTransfer,
		ResourceCreation, ResourceUpdation, ResourceDeletion, ResourceLinkCreation, ResourceLinkDeletion,
		AnalysisResultUpdation
	}

	public abstract class Operation : ITypeCode, IBinarySerializable
	{
		public string			Error;
		public int				ExeUnits;
		public Money			Reward;
		public Transaction		Transaction;
		public AccountAddress	Signer => Transaction.Signer;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}

		public const string		Rejected = "Rejected";
		public const string		NotFound = "Not found";
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

		public static Money CalculateFee(Money rentperbyteperday, int length, Time time)
		{
			return rentperbyteperday * length * Mcv.TimeFactor(time);
		}

		public void Pay(Round round, int length, Time time)
		{
			var fee = CalculateFee(round.RentPerBytePerDay, length, time);
			
			Affect(round, Signer).Balance -= fee;
		}

		public void Allocate(Round round, Domain domain, int toallocate)
		{
			if(domain.SpaceReserved < domain.SpaceUsed + toallocate)
			{
				Pay(round, domain.SpaceUsed + toallocate - domain.SpaceReserved, domain.Expiration - round.ConsensusTime);
	
				domain.SpaceReserved = 
				domain.SpaceUsed = (short)(domain.SpaceUsed + toallocate);
			}
			else
				domain.SpaceUsed += (short)toallocate;
		}

		public void Free(Domain domain, int toallocate)
		{
			domain.SpaceUsed -= (short)toallocate;
		}

		public AccountEntry Affect(Round round, AccountAddress account)
		{
			var e = round.Mcv.Accounts.Find(account, round.Id);	

			if(e == null) /// new Account
			{
				Pay(round, Mcv.EntityLength, Mcv.Forever);
			}

			return round.AffectAccount(account);
		}

		public DomainEntry Affect(Round round, string domain)
		{
			return round.AffectDomain(domain);
		}

		public bool RequireDomain(Round round, AccountAddress signer, string name, out DomainEntry domain)
		{
			domain = round.Mcv.Domains.Find(name, round.Id);

			if(domain == null)
			{
				Error = NotFound;
				return false;
			}

			if(Domain.IsExpired(domain, round.ConsensusTime))
			{
				Error = Expired;
				return false;
			}

			if(signer != null && domain.Owner != signer)
			{
				Error = NotOwner;
				return false;
			}

			return true;
		}

		public bool Require(Round round, AccountAddress signer, ResourceAddress address, out DomainEntry domain, out Resource resource)
		{
			resource = null;

			if(RequireDomain(round, signer, address.Domain, out domain) == false)
			{
				Error = NotFound;
				return false; 
			}

			resource = domain.Resources.FirstOrDefault(i => i.Address.Resource == address.Resource);
			
			if(resource == null)
			{
				Error = NotFound;
				return false; 
			}

			return true; 
		}
	}

	public class OperationJsonConverter : JsonConverter<Operation>
	{
		public override Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString().Split(':');
			var o = Operation.FromType(Enum.Parse<OperationClass>(s[0]));
 			
			o.Read(new BinaryReader(new MemoryStream(s[1].FromHex())));

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

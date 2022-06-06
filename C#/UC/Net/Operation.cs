using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;

namespace UC.Net
{
	public enum DelegationStage
	{
		Null, Pending, Delegated, Completed
	}

	public enum PlacingStage
	{
		Null, Accepted, Pending, Placed, Confirmed, FailedOrNotFound
	}

	public struct Portion
	{
		public Coin	Factor;
		public Coin	Amount;
	}

	public enum Operations
	{
		Null = 0, CandidacyDeclaration, Emission, UntTransfer, AuthorBid, AuthorRegistration, AuthorTransfer, ProductRegistration, ReleaseManifest 
	}

	public interface IFlowControl
	{
		Log			Log { get; }
		void		StageChanged();
		void		SetOperation(Operation o);
	}

	public abstract class Operation
	{
		public int				Id;
		public string			Error;
		public Account			Signer { get; set; }
		public Transaction		Transaction;
		public DelegationStage	Delegation;
		public PlacingStage		Placing;
		public bool				Successful => Executed && Error == null;
		public IFlowControl		FlowReport;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}
		public bool				Executed; 

		public const string		Rejected = "Rejected";
		public const string		NotEnoughUNT = "Not enough UNT";
		public const string		SignerDoesNotOwnTheAuthor = "The signer does not own the Author";

		public Operations		Type => Enum.Parse<Operations>(GetType().Name);

		public Operation()
		{
		}

		public static Operation FromType(Operations type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(GetMembersRequest).Namespace + "." + type).GetConstructor(new System.Type[]{}).Invoke(new object[]{}) as Operation;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Operation)} type", ex);
			}
		}

		public virtual void Execute(Roundchain chain, Round round)
		{
		}
 
		public override string ToString()
		{
			return $"{Type}, Id={Id}, {Delegation}, {Placing}, " + Description + $", Error={Error}";
		}

		public virtual void Read(BinaryReader r)
		{
			Id = r.Read7BitEncodedInt();
		}

		public virtual void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Id);
		}

		public virtual void HashWrite(BinaryWriter w)
		{
			Write(w);
		}

		public virtual void WritePaid(BinaryWriter w)
		{
			Write(w);
		}

		public virtual void WriteConfirmed(BinaryWriter w)
		{
			Write(w);
		}

		public virtual void ReadConfirmed(BinaryReader r)
		{
			Placing = PlacingStage.Confirmed;
			Read(r);
		}

		public static bool IsValid(string author, string title)
		{
			if(author.Length == 0)
				return false;

			var r = new Regex(@"^[a-z0-9_]+$");
			
			if(r.Match(author).Success == false)
				return false;

			if(TitleToName(title) != author)
				return false;

			return true;
		}

		public static string TitleToName(string title)
		{
			return Regex.Matches(title, @"[a-zA-Z0-9_]+").Aggregate(string.Empty, (a,m) => a += m.Value).ToLower();
		}
		
		public Coin CalculateFee(Coin factor)
		{
			var s = new MemoryStream(); 
			var w = new BinaryWriter(s);

			WritePaid(w); 

			return Roundchain.FeePerByte * ((Emission.FactorEnd - factor) / Emission.FactorEnd) * (int)s.Length;
		}
		
		public static Coin CalculateFee(Coin factor, IEnumerable<Operation> operations)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(operations, i => {
									 	i.WritePaid(w); 
									 });

			return Roundchain.FeePerByte * ((Emission.FactorEnd - factor) / Emission.FactorEnd) * (int)s.Length;
		}

	}

	public class CandidacyDeclaration : Operation
	{
		public Coin				Bail;
		public IPAddress		IP;
		public override string	Description => $"{Bail} UNT";
		public override bool Valid => Settings.Dev.DisableBailMin ? true : Bail >= Roundchain.BailMin;
		
		public CandidacyDeclaration()
		{
		}

		public CandidacyDeclaration(PrivateAccount signer, Coin bail, IPAddress ip)
		{
			//if(!Settings.Dev.DisableBailMin && bail < Roundchain.BailMin)	throw new RequirementException("The bail must be greater than or equal to BailMin");

			Signer = signer;
			Bail = bail;
			IP = ip;
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Bail	= r.ReadCoin();
			IP		= new IPAddress(r.ReadBytes(4));
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Bail);
			w.Write(IP.GetAddressBytes());
		}

		public override void Execute(Roundchain chain, Round round)
		{
			var e = round.ChangeAccount(Signer);

			var prev = e.FindOperation<CandidacyDeclaration>(round);

			if(prev != null && e.BailStatus == BailStatus.OK) /// first, add existing if not previously Siezed
				e.Balance += prev.Bail;

			e.Balance -= Bail; /// then, subtract a new bail
			e.Bail += Bail;

			if(e.BailStatus == BailStatus.Siezed) /// if was siezed than reset to OK status
				e.BailStatus = BailStatus.OK;
		}
	}

	public class Emission : Operation
	{
		public static readonly Coin		FactorStart = 0;
		public static readonly Coin		FactorEnd = 1000;
		public static readonly Coin		FactorStep = new Coin(0.1);
		public static readonly Coin		Step = 1000;

		public BigInteger	Wei;
		public int			Eid;

		public override string Description => $"#{Eid} {Nethereum.Web3.Web3.Convert.FromWeiToBigDecimal(Wei).ToString()} ETH -> {(Portion.Amount > 0 ? Portion.Amount : "???")} UNT";

		Portion Portion;

		public Emission() 
		{
		}

		public Emission(PrivateAccount signer, BigInteger wei, int eid)
		{
			Signer = signer;
			Wei = wei;
			Eid = eid;
		}

		public override bool Valid => 0 < Wei && 0 <= Eid;

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Wei	= r.ReadBigInteger();
			Eid	= r.Read7BitEncodedInt();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Wei);
			w.Write7BitEncodedInt(Eid);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			Portion = Calculate(round.WeiSpent, round.Factor, Wei);
			
			if(Portion.Factor < FactorEnd)
			{
				round.ChangeAccount(Signer).Balance += Portion.Amount;
				round.Distribute(Portion.Amount * 1/10, round.Fundables);

				round.Factor = Portion.Factor;
				round.WeiSpent += Wei;
				round.Emission += Portion.Amount;
			}
			else
				Error = "Emission is over"; /// emission is over
		}

		public static byte[] Serialize(Account beneficiary, int eid)
		{ 
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(beneficiary);
			w.Write7BitEncodedInt(eid);

			return s.ToArray();
		}
		
		public static Coin Multiplier(Coin factor)
		{
			return FactorEnd - factor;
		}

		public static Portion Calculate(BigInteger spent, Coin factor, BigInteger wei)
		{
			Coin f = factor;
			Coin a = 0;

			var d = Step - Coin.FromWei(spent) % Step;

			var w = Coin.FromWei(wei);

			if(w <= d)
			{
				a += w * Multiplier(f);
			}
			else
			{
				a += d * Multiplier(f);

				var r = w - d;

				while(f < FactorEnd)
				{
					f = f + FactorStep;

					if(r < Step)
					{
						a += r * Multiplier(f);
						break;
					}
					else
					{
						a += Step * Multiplier(f);
						r -= Step;
					}
				}
			} 

			return new Portion { Factor = f, Amount = a};
		}
	}

	public class UntTransfer : Operation
	{
		public Account		To;
		public Coin			Amount;
		public override string Description => $"{Amount} UNT -> {To}";

		public UntTransfer()
		{
		}

		public UntTransfer(PrivateAccount signer, Account to, Coin amount)
		{
			if(signer == null)	throw new RequirementException("Source account is null or invalid");
			if(to == null)		throw new RequirementException("Destination account is null or invalid");

			Signer = signer;
			To = to;
			Amount = amount;
		}

		public override bool Valid => 0 <= Amount;

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			To		= r.ReadAccount();
			Amount	= r.ReadCoin();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(To);
			w.Write(Amount);
		}

		public override void  Execute(Roundchain chain, Round round)
		{
			round.ChangeAccount(Signer).Balance -= Amount;
			round.ChangeAccount(To).Balance += Amount;
		}
	}

	public class AuthorBid : Operation
	{
		public string			Author;
		public Coin				Bid {get; set;}
		public override string	Description => $"{Bid} UNT for {Author}";
		public override bool	Valid => Author.Length > 0 && AuthorEntry.IsExclusive(Author) && (Settings.Dev.DisableBidMin || GetMinCost(Author) <= Bid);

		public AuthorBid()
		{
		}

		public AuthorBid(PrivateAccount signer, string name, Coin bid)
		{
			Signer = signer;
			Author = name;
			Bid = bid;
		}
		
		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Author	= r.ReadUtf8();
			Bid		= r.ReadCoin();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.WriteUtf8(Author);
			w.Write(Bid);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			var a = round.ChangeAuthor(Author);

			ChainTime sinceauction() => round.Time - a.FirstBidTime/* fb.Transaction.Payload.Round.Time*/;

			bool expired = a.LastWinner != null && (a.Owner == null && sinceauction() > ChainTime.FromYears(2) ||		/// winner has not registered since the end of auction																/// winner has not registered during 2 year since auction start, restart the auction
													a.Owner != null && round.Time - a.RegistrationTime > ChainTime.FromYears(a.Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			if(a.LastWinner == null || sinceauction() < ChainTime.FromYears(1))
	 			{
					if(a.LastWinner == null) /// first bid
					{
						round.ChangeAccount(Signer).Balance -= Bid;
						
						a.FirstBidTime = round.Time;
						a.LastBid = Bid;
						a.LastBidTime = round.Time;
						a.LastWinner = Signer;
						
						return;
					}
					else
					{
						//var lb = a.FindLastBid(round);
	
						if(a.LastBid < Bid) /// outbid
						{
							round.ChangeAccount(a.LastWinner).Balance += a.LastBid;
							round.ChangeAccount(Signer).Balance -= Bid;
							
							a.LastBid = Bid;
							a.LastBidTime = round.Time;
							a.LastWinner = Signer;
				
							return;
						}
					}
	 			}
 			} 
 			else
 			{
				//var lr = a.FindRegistration(round);

				if(a.Owner != null)
				{
					round.ChangeAccount(a.Owner).Authors.Remove(Author);
					a.Owner = null;
				}

				/// dont refund previous winner

				//var wb = a.FindLastBid(round);
				round.Distribute(a.LastBid, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);

				round.ChangeAccount(Signer).Balance -= Bid;
				
				a.FirstBidTime = round.Time;
				a.LastBid = Bid;
				a.LastBidTime = round.Time;
				a.LastWinner = Signer;
				a.Products.Clear();
			
				return;
			}

			Error = "Bid too low or auction is over";
		}

		public static Coin GetMinCost(string name)
		{
			Coin c = 1;
			
			for(int i = 0; i < 4 - name.Length; i++)
				c *= 100;

			return c;
		}
	}

	public class AuthorRegistration : Operation
	{
		
		public string				Author;
		public string				Title {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => AuthorEntry.IsExclusive(Author); 
		public override string		Description => $"{Author} ({Title}) for {Years} years";
		public override bool		Valid => IsValid(Author, Title) && 0 < Years;
		
		public static Coin			GetCost(Round round, int years) => new Coin(1)/1000 * years * (Emission.FactorEnd - round.Factor) / Emission.FactorEnd;

		public AuthorRegistration()
		{
		}

		public AuthorRegistration(PrivateAccount signer, string author, string title, byte years)
		{
			if(!Operation.IsValid(author, title))
				throw new ArgumentException("Invalid Author name/title");

			Signer = signer;
			Author = author;
			Title = title;
			Years = years;
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Author	= r.ReadUtf8();
			Title	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.WriteUtf8(Author);
			w.WriteUtf8(Title);
			w.Write(Years);
		}

		public override void Execute(Roundchain chain, Round round)
		{
//			AuthorBid lb = null;

			var a = round.FindAuthor(Author);

// 			if(Exclusive)
// 			{
// 				lb = a?.FindLastBid(round);
// 			}
// 
// 			var lr = a?.FindRegistration(round);

			ChainTime sinceauction() => round.Time - a.FirstBidTime;
			ChainTime sincelastreg() => round.Time - a.RegistrationTime;
						
			if(	a == null && !Exclusive ||																																			/// available
				a != null && !Exclusive && sincelastreg() > ChainTime.FromYears(a.Years)							||																				/// not renewed
				a != null && Exclusive && a.LastWinner == Signer && a.Owner == null && ChainTime.FromYears(1) < sinceauction() && sinceauction() < ChainTime.FromYears(2) ||	/// auction is over and a winner can register the author during 1 year
				a != null && a.Owner == Signer && sincelastreg() < ChainTime.FromYears(a.Years) 																		/// renew
			   )	
			{
				if(a?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						round.Distribute(a.LastBid, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);

					round.ChangeAccount(Signer).Authors.Add(Author);
				}

				var cost = GetCost(round, Years);


				a = round.ChangeAuthor(Author);
				a.Obtained = round.Id;
				a.Title = Title;
				a.Owner = Signer;
				a.RegistrationTime = round.Time;
				a.Years = Years;

				round.ChangeAccount(Signer).Balance -= cost;
				round.Distribute(cost, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);
			}
			else
				Error = "Failed";
		}
	}

	public class AuthorTransfer : Operation
	{
		public string			Author;
		public Account			To  {get; set;}
		public override string	Description => $"{Author} -> {To}";
		public override bool	Valid => 0 < Author.Length;
		
		public AuthorTransfer()
		{
		}

		public AuthorTransfer(PrivateAccount signer, string name, Account to)
		{
			Signer = signer;
			Author = name;
			To = to;
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Author	= r.ReadUtf8();
			To		= r.ReadAccount();
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.WriteUtf8(Author);
			w.Write(To);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			if(!round.ChangeAccount(Signer).Authors.Contains(Author))
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			round.ChangeAccount(Signer).Authors.Remove(Author);
			round.ChangeAccount(To).Authors.Add(Author);

			round.ChangeAuthor(Author).Obtained = round.Id;
			round.ChangeAuthor(Author).Owner = To;
		}
	}

	public class ProductRegistration : Operation
	{
		public ProductAddress	Address;
		public string			Title;
		public override string	Description => $"{Address} as {Title}";
		public override bool	Valid => 0 < Address.Author.Length && 0 < Address.Product.Length;

		public ProductRegistration()
		{
		}

		public ProductRegistration(PrivateAccount signer, ProductAddress name, string title)
		{
			Signer		= signer;
			Address		= name;
			Title		= title;
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Address	= r.Read<ProductAddress>();
			Title	= r.ReadUtf8();

			if(Title== null)
			{
				Title = Title;
			}
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			if(Title== null)
			{
				Title = Title;
			}

			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public override void Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}


			if(!a.Products.Contains(Address.Product))
			{
				a = round.ChangeAuthor(Address.Author);
				///a.Rid = round.Id;
				a.Products.Add(Address.Product);
			}
			 
			var p = round.ChangeProduct(Address);
		
			p.Title				= Title;
			p.LastRegistration	= round.Id;
		}
	}

	public class ProductControl : Operation
	{
		enum Change
		{
			AddPublisher, RemovePublisher, SetStatus
		}

		public ProductAddress		Product;
		public string				Class; /// Application, Library, Component(Add-on/Plugin), Font, etc.
		public ProductAddress		Master; /// For Components
		public string				LogoAddress;
		Dictionary<Change, object>	Actions;
		public override string		Description => $"{Product} ...";

		public ProductControl()
		{
		}

		public ProductControl(PrivateAccount signer, ProductAddress product)
		{
			Signer		= signer;
			Product		= product;
			Actions		= new();
		}

		public override bool Valid => Product.Valid;

		public override string	ToString()							=> base.ToString() + $", {Product}";
		public void				AddPublisher(Account publisher)		=> Actions[Change.AddPublisher] = publisher;
		public void				RemovePublisher(Account publisher)	=> Actions[Change.RemovePublisher] = publisher;
		public void				SetStatus(bool active)				=> Actions[Change.SetStatus] = active;

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Product	= r.Read<ProductAddress>();
			Actions = r.ReadDictionary(() =>{
												var k = (Change)r.ReadByte();	
												var o = new KeyValuePair<Change, object>(k,	k switch {
																										Change.AddPublisher => r.ReadAccount(),
																										Change.RemovePublisher => r.ReadAccount(),
																										Change.SetStatus => r.ReadBoolean(),
																										_ => throw new IntegrityException("Wrong ProductControl.Change")
																									 });
												return o;
											});
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Product);
			w.Write(Actions, i =>	{
										w.Write((byte)i.Key);

										switch(i.Key)
										{
											case Change.AddPublisher:		w.Write(i.Value as Account); break;
											case Change.RemovePublisher:	w.Write(i.Value as Account); break;
											case Change.SetStatus:			w.Write((bool)i.Value); break;
										}
									});
		}
	}

		
	public class ReleaseManifest : Operation, IBinarySerializable
	{
		public ReleaseAddress			Address;
		public string					Channel;		/// stable, beta, nightly, debug,...
		public Version					PreviousVersion;
		public byte[]					CompleteHash;
		public long						CompleteSize;
		public ReleaseAddress[]			CompleteDependencies;
		public Version					IncrementalMinimalVersion;
		public byte[]					IncrementalHash;
		public long						IncrementalSize;
		public ReleaseAddress[]			IncrementalDependencies;


		public override bool			Valid => Address.Valid;
		public override string			Description => $"{Address}/{Channel}";

		byte[]							Hash;
		bool							Archived;

		public ReleaseManifest()
		{
		}

		public ReleaseManifest(	PrivateAccount				signer, 
								ReleaseAddress				address, 
								string						channel, 
								Version						previous, 
								long						completesize,
								byte[]						completehash,
								IEnumerable<ReleaseAddress>	completedependencies, 
								Version						incrementalminimalversion, 
								long						incrementalsize,
								byte[]						incrementalhash,
								IEnumerable<ReleaseAddress>	incrementaldependencies)
		{
			Signer	= signer;
			Address = address;
			Channel = channel;
			PreviousVersion = previous;
			CompleteSize = completesize;
			CompleteHash = completehash;
			CompleteDependencies = completedependencies.ToArray();
			IncrementalMinimalVersion = incrementalminimalversion;
			IncrementalHash = incrementalhash;
			IncrementalSize = incrementalsize;
			IncrementalDependencies = incrementaldependencies.ToArray();
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			d.Add("Address").Value = Address;
			d.Add("Channel").Value = Channel;
			d.Add("PreviousVersion").Value = PreviousVersion;

			d.Add("CompleteHash").Value = Hex.ToHexString(CompleteHash);
			d.Add("CompleteSize").Value = CompleteSize;

			if(CompleteDependencies.Any())
			{
				var cd = d.Add("CompleteDependencies");
				foreach(var i in CompleteDependencies)
				{
					cd.Add(i.ToString());
				}
			}

			if(IncrementalSize > 0)
			{
				d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;
				d.Add("IncrementalHash").Value = Hex.ToHexString(IncrementalHash);
				d.Add("IncrementalSize").Value = IncrementalSize;
	
				if(IncrementalDependencies.Any())
				{
					var id = d.Add("IncrementalDependencies");
					foreach(var i in IncrementalDependencies)
					{
						id.Add(i.ToString());
					}
				}
			}

			d.Add("Hash").Value = Hex.ToHexString(Hash);

			return d;		
		}

		public byte[] GetOrCalcHash()
		{
			if(Hash != null)
			{
				return Hash;
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
	
			w.Write(Address);
			w.WriteUtf8(Channel);
			w.Write(PreviousVersion);
			w.Write(CompleteHash);
			w.Write7BitEncodedInt64(CompleteSize);
			w.Write(CompleteDependencies);
			
			w.Write7BitEncodedInt64(IncrementalSize);

			if(IncrementalSize > 0)
			{
				w.Write(IncrementalMinimalVersion);
				w.Write(IncrementalHash);
				w.Write(IncrementalDependencies);
			}
				
			Hash = Cryptography.Current.Hash(s.ToArray());
		
			return Hash;
		}

		public override void HashWrite(BinaryWriter writer)
		{
			writer.Write(GetOrCalcHash());
		}

		public override void WritePaid(BinaryWriter w)
		{
			w.Write(GetOrCalcHash());
		}

		public override void Read(BinaryReader r)
		{
			base.Read(r);

			Archived = r.ReadBoolean();

			if(Archived)
			{
				Hash = r.ReadSha3();
			} 
			else
			{
				Address = r.Read<ReleaseAddress>();
				Channel = r.ReadUtf8();
				PreviousVersion = r.ReadVersion();

				CompleteSize = r.Read7BitEncodedInt64();
				CompleteHash = r.ReadSha3();
				CompleteDependencies = r.ReadArray<ReleaseAddress>();

				IncrementalSize = r.Read7BitEncodedInt64();
				
				if(IncrementalSize > 0)
				{
					IncrementalMinimalVersion = r.ReadVersion();
					IncrementalHash = r.ReadSha3();
					IncrementalDependencies = r.ReadArray<ReleaseAddress>();
				}

				Hash = GetOrCalcHash();
			}
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Archived);

			if(Archived)
			{
				w.Write(GetOrCalcHash());
			} 
			else
			{
				w.Write(Address);
				w.WriteUtf8(Channel);
				w.Write(PreviousVersion);

				w.Write7BitEncodedInt64(CompleteSize);
				w.Write(CompleteHash);
				w.Write(CompleteDependencies);
			
				w.Write7BitEncodedInt64(IncrementalSize);

				if(IncrementalSize > 0)
				{
					w.Write(IncrementalMinimalVersion);
					w.Write(IncrementalHash);
					w.Write(IncrementalDependencies);
				}
			}
		}

		public override void Execute(Roundchain chain, Round round)
		{
			if(Archived)
				return;

			var a = round.FindAuthor(Address.Author);

			if(a == null || a.Owner != Signer)
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			if(!a.Products.Contains(Address.Product))
			{
				Error = "Product is not registered";
				return;
			}
 
			var p = round.FindProduct(Address);

			if(p == null)
				throw new IntegrityException("Product not found");
	
			var r = p.Releases.FirstOrDefault(i => i.Platform == Address.Platform && i.Channel == Channel);
					
			if(r != null)
			{
				if(r.Version < Address.Version)
				{
					var prev = chain.FindRound(r.Rid).FindOperation<ReleaseManifest>(m =>	m.Address.Author == Address.Author && 
																							m.Address.Product == Address.Product && 
																							m.Address.Platform == Address.Platform && 
																							m.Channel == Channel);
					if(prev == null)
						throw new IntegrityException("No ReleaseDeclaration found");
					
					p = round.ChangeProduct(Address);

					prev.Archived = true;
					round.AffectedRounds.Add(prev.Transaction.Payload.Round);
					p.Releases.Remove(r);

				} 
				else
				{
					Error = "Version must be greater than current";
					return;
				}
			}
			else
				p = round.ChangeProduct(Address);


			//var rls = round.GetReleases(round.Id);
			//rls.Add(this);

			p.Releases.Add(new Release(Address.Platform, Address.Version, Channel, round.Id));
		}
	}
}

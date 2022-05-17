using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;

namespace UC.Net
{
	public enum DelegationStage
	{
		Null, Pending, Delegated, Failed, Confirmed
	}

	public enum PlacingStage
	{
		Null, NotFoundOrFailed, Accepted, Pending, Placed, Confirmed
	}

	public struct Portion
	{
		public Coin	Factor;
		public Coin	Amount;
	}

	public enum Operations
	{
		Null = 0, CandidacyDeclaration, Emission, UntTransfer, AuthorBid, AuthorRegistration, AuthorTransfer, ProductRegistration, ReleaseDeclaration 
	}

// 	public enum OperationArgument : byte
// 	{
// 		Null = 0, Coin, Integer, Bytes, String, Date, Version, Account
// 	}

	public enum OperationResult : byte
	{
		Null = 0, OK, Failed, Rejected
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
		public OperationResult	Result;
		public Account			Signer { get; set; }
		public Transaction		Transaction;
		public DelegationStage	Delegation;
		public PlacingStage		Placing;
		public bool				Successful => Result == OperationResult.OK;
		public IFlowControl		FlowReport;
		public abstract string	Description { get; }
		public abstract bool	Valid {get;}
		//public virtual bool		Mutable { get => false; } 

		public static Operation FromType(Operations type)
		{
			return	type switch
					{
						Operations.CandidacyDeclaration	=> new CandidacyDeclaration(),
						Operations.Emission				=> new Emission(),
						Operations.UntTransfer			=> new UntTransfer(),
						Operations.AuthorBid			=> new AuthorBid(),
						Operations.AuthorRegistration	=> new AuthorRegistration(),
						Operations.AuthorTransfer		=> new AuthorTransfer(),
						Operations.ProductRegistration	=> new ProductRegistration(),
						Operations.ReleaseDeclaration	=> new ReleaseManifest(),
						_ => throw new IntegrityException("Wrong operation type")
					};
		}

		public Operations Type
		{
			get
			{
				return	this switch
						{
							CandidacyDeclaration	=> Operations.CandidacyDeclaration,
							Emission 				=> Operations.Emission,
							UntTransfer				=> Operations.UntTransfer,
							AuthorBid				=> Operations.AuthorBid,
							AuthorRegistration		=> Operations.AuthorRegistration,
							AuthorTransfer			=> Operations.AuthorTransfer,
							ProductRegistration		=> Operations.ProductRegistration,
							ReleaseManifest			=> Operations.ReleaseDeclaration,
							_ => throw new IntegrityException("Wrong operation type")
						};
			}
		}

		public Operation()
		{
		}

		public virtual OperationResult Execute(Roundchain chain, Round round)
		{
			return OperationResult.OK;
		}
 
		public override string ToString()
		{
			return $"{Type}, {Id}, {Result}, " + Description;
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
		public override bool Valid => Transaction.Settings.Dev.DisableBailMin ? true : Bail >= Roundchain.BailMin;
		
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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			var e = round.GetAccount(Signer);

			var prev = e.FindOperation<CandidacyDeclaration>(round);

			if(prev != null && e.BailStatus == BailStatus.OK) /// first, add existing if not previously Siezed
				e.Balance += prev.Bail;

			e.Balance -= Bail; /// then, subtract a new bail
			e.Bail += Bail;

			if(e.BailStatus == BailStatus.Siezed) /// if was siezed than reset to OK status
				e.BailStatus = BailStatus.OK;

			return e.Balance >= 0 ? OperationResult.OK : OperationResult.Failed;
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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			Portion = Calculate(round.WeiSpent, round.Factor, Wei);
			
			if(Portion.Factor < FactorEnd)
			{
				round.GetAccount(Signer).Balance += Portion.Amount;
				round.Distribute(Portion.Amount * 1/10, round.Fundables);

				round.Factor = Portion.Factor;
				round.WeiSpent += Wei;
				round.Emission += Portion.Amount;

				return OperationResult.OK;
			}
			else
				return OperationResult.Failed; /// emission is over
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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			round.GetAccount(Signer).Balance -= Amount;
			round.GetAccount(To).Balance += Amount;

			return round.AffectedAccounts[Signer].Balance >= 0 ? OperationResult.OK : OperationResult.Failed;
		}
	}

	public class AuthorBid : Operation
	{
		public string			Author;
		public Coin				Bid {get; set;}
		public override string	Description => $"{Bid} UNT for {Author}";
		public override bool	Valid => Author.Length > 0 && Author.Length <= AuthorRegistration.LengthMaxForAuction && (Transaction.Settings.Dev.DisableBidMin ? true : GetMinCost(Author) <= Bid);

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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			var a = round.GetAuthor(Author);

			ChainTime sinceauction() => round.Time - chain.FindRound(a.FirstBid).Time/* fb.Transaction.Payload.Round.Time*/;

			bool expired = a.FirstBid != -1 && (a.LastRegistration == -1 && sinceauction() > ChainTime.FromYears(2) ||																		/// winner has not registered during 2 year since auction start, restart the auction
												a.LastRegistration != -1 && round.Time - chain.FindRound(a.LastRegistration).Time > ChainTime.FromYears(a.FindRegistration(round).Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			if(a.FirstBid == -1 || sinceauction() < ChainTime.FromYears(1))
	 			{
					if(a.FirstBid == -1) /// first bid
					{
						round.GetAccount(Signer).Balance -= Bid;
						a.FirstBid = round.Id;
						a.LastBid = round.Id;
	
						return OperationResult.OK;
					}
					else
					{
						var lb = a.FindLastBid(round);
	
						if(lb.Bid < Bid) /// outbid
						{
							round.GetAccount(lb.Signer).Balance += lb.Bid;
							round.GetAccount(Signer).Balance -= Bid;
							a.LastBid = round.Id;
					
							return OperationResult.OK;
						}
					}
	 			}
 			} 
 			else
 			{
				var lr = a.FindRegistration(round);

				if(lr != null)
				{
					round.GetAccount(lr.Signer).Authors.Remove(Author);
				}

				/// dont refund previous winner

				var wb = a.FindLastBid(round);
				round.Distribute(wb.Bid, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);

				round.GetAccount(Signer).Balance -= Bid;
				a.FirstBid			= round.Id;
				a.LastBid			= round.Id;
				a.LastRegistration	= -1;
				a.LastTransfer		= -1;
				a.Products.Clear();
			
				return OperationResult.OK;
			}

			return OperationResult.Failed;
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
		public const int			LengthMaxForAuction = 4;
		
		public string				Author;
		public string				Title {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => Author.Length <= LengthMaxForAuction; 
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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			AuthorBid lb = null;

			var a = round.FindAuthor(Author);

			if(Exclusive)
			{
				lb = a?.FindLastBid(round);
			}

			var lr = a?.FindRegistration(round);

			ChainTime sinceauction() => round.Time - chain.FindRound(a.FirstBid).Time;
			ChainTime sincelastreg() => round.Time - lr.Transaction.Payload.Round.Time;
						
			if(!Exclusive && a == null ||																																			/// available
			   !Exclusive && sincelastreg() > ChainTime.FromYears(lr.Years)							||																				/// not renewed
				Exclusive && lb != null && lb.Signer == Signer && a.LastRegistration == -1 && ChainTime.FromYears(1) < sinceauction() && sinceauction() < ChainTime.FromYears(2) ||	/// auction is over and a winner can register the author during 1 year
				a != null && a.FindOwner(round) == Signer && sincelastreg() < ChainTime.FromYears(lr.Years) 																		/// renew
			   )	
			{
				if(round.GetAuthor(Author).LastRegistration == -1)
				{
					if(Exclusive) /// distribite winner bid, one time
						round.Distribute(lb.Bid, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);

					round.GetAccount(Signer).Authors.Add(Author);
				}

				var cost = GetCost(round, Years);

				round.GetAccount(Signer).Balance -= cost;
				round.GetAuthor(Author).LastRegistration = round.Id;
				round.Distribute(cost, round.Members.Select(i => i.Generator), 1, round.Fundables, 1);

				return OperationResult.OK;
			}

			return OperationResult.Failed;
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

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			round.GetAccount(Signer).Authors.Remove(Author);
			round.GetAccount(To).Authors.Add(Author);
			round.GetAuthor(Author).LastTransfer = round.Id;

			return OperationResult.OK;
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
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(Address);
			w.WriteUtf8(Title);
		}

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.FindOwner(round) != Signer)
				return OperationResult.Failed;

			if(!a.Products.Contains(Address.Product))
				a.Products.Add(Address.Product);
 
			var p = round.GetProduct(Address);
		
			p.Title				= Title;
			p.LastRegistration	= round.Id;

			return OperationResult.OK;
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
		public Version					MinimalVersion;
		public List<ReleaseAddress>		CompleteDependencies;
		public List<ReleaseAddress>		IncrementalDependencies;

		public override bool			Valid => Address.Valid;
		public override string			Description => $"{Address}/{Channel}";

		byte[]							Hash;
		bool							HashOnly;

		public ReleaseManifest()
		{
		}

		public ReleaseManifest(PrivateAccount signer, ReleaseAddress address, string channel, Version previous, Version minimal, List<ReleaseAddress> completeDependencies, List<ReleaseAddress> incrementalDependencies)
		{
			Signer	= signer;
			Address = address;
			Channel = channel;
			PreviousVersion = previous;
			MinimalVersion = minimal;
			CompleteDependencies = completeDependencies;
			IncrementalDependencies = incrementalDependencies;
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
			w.Write(MinimalVersion);
			w.Write(CompleteDependencies);
			w.Write(IncrementalDependencies);
	
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

			HashOnly = r.ReadBoolean();

			if(HashOnly)
			{
				Hash = r.ReadSha3();
			} 
			else
			{
				Address = r.Read<ReleaseAddress>();
				Channel = r.ReadUtf8();
				PreviousVersion = r.ReadVersion();
				MinimalVersion = r.ReadVersion();
				CompleteDependencies = r.ReadList<ReleaseAddress>();
				IncrementalDependencies = r.ReadList<ReleaseAddress>();

				Hash = GetOrCalcHash();
			}
		}

		public override void Write(BinaryWriter w)
		{
			base.Write(w);

			w.Write(HashOnly);

			if(HashOnly)
			{
				w.Write(GetOrCalcHash());
			} 
			else
			{
				w.Write(Address);
				w.WriteUtf8(Channel);
				w.Write(PreviousVersion);
				w.Write(MinimalVersion);
				w.Write(CompleteDependencies);
				w.Write(IncrementalDependencies);
			}
		}

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Address.Author);

			if(a == null || a.FindOwner(round) != Signer)
				return OperationResult.Failed;

			if(!a.Products.Contains(Address.Product))
				return OperationResult.Failed;
 
			var p = round.GetProduct(Address);
	
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
					
					prev.HashOnly = true;
					round.AffectedRounds.Add(prev.Transaction.Payload.Round);
					p.Releases.Remove(r);

				} 
				else
					return OperationResult.Failed;
			}

			//var rls = round.GetReleases(round.Id);
			//rls.Add(this);

			p.Releases.Add(new Release(Address.Platform, Address.Version, Channel, round.Id));

			return OperationResult.OK;
		}
	}
}

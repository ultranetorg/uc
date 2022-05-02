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
	public enum ProcessingStage
	{
		Null, Accepted, Pending, Delegated, Placed, Confirmed
	}

	public struct Portion
	{
		public Coin	Factor;
		public Coin	Amount;
	}

	public enum Operations
	{
		Null = 0, CandidacyDeclaration, Emission, UntTransfer, AuthorBid, AuthorRegistration, AuthorTransfer, ProductRegistration, 
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
		public OperationResult	Result;
		public Account			Signer { get; set; }
		public Transaction		Transaction;
		public ProcessingStage	Stage;
		public bool				Successful => Result == OperationResult.OK;
		public IFlowControl		FlowReport;
		public abstract string	Description { get; }

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
			return $"{Type}, " + Description;
		}

		public abstract void Read(BinaryReader r);
		public abstract void Write(BinaryWriter w);
		public abstract bool IsValid();
		public virtual void HashWrite(BinaryWriter w)
		{
			Write(w);
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
	}

	public class CandidacyDeclaration : Operation
	{
		public Coin				Bail;
		public IPAddress		IP;
		public override string	Description => $"{Bail} UNT";
		
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

		public override bool IsValid()
		{
			return Transaction.Settings.Dev.DisableBailMin ? true : Bail >= Roundchain.BailMin;
		}

		public override void Read(BinaryReader r)
		{
			Bail	= r.ReadCoin();
			IP		= new IPAddress(r.ReadBytes(4));
		}

		public override void Write(BinaryWriter w)
		{
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

		public override bool IsValid()
		{
			return 0 < Wei && 0 <= Eid;
		}

		public override void Read(BinaryReader r)
		{
			Wei	= r.ReadBigInteger();
			Eid	= r.Read7BitEncodedInt();
		}

		public override void Write(BinaryWriter w)
		{
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

		public override bool IsValid()
		{
			return 0 <= Amount;
		}

		public override void Read(BinaryReader r)
		{
			To		= r.ReadAccount();
			Amount	= r.ReadCoin();
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(To);
			w.Write(Amount);
		}

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			round.GetAccount(Signer).Balance -= Amount;
			round.GetAccount(To).Balance += Amount;

			return round.Accounts[Signer].Balance >= 0 ? OperationResult.OK : OperationResult.Failed;
		}
	}

	public class AuthorBid : Operation
	{
		public string			Author;
		public Coin				Bid {get; set;}
		public override string	Description => $"{Bid} UNT for {Author}";

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
			Author	= r.ReadUtf8();
			Bid		= r.ReadCoin();
		}

		public override void Write(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.Write(Bid);
		}

		public override bool IsValid()
		{
			return Author.Length > 0 && Author.Length <= AuthorRegistration.LengthMaxForAuction && (Transaction.Settings.Dev.DisableBidMin ? true : GetMinCost(Author) <= Bid);
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

		public override bool IsValid()
		{
			return IsValid(Author, Title) && 0 < Years;
		}

		public override void Read(BinaryReader r)
		{
			Author	= r.ReadUtf8();
			Title	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		public override void Write(BinaryWriter w)
		{
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
		
		public AuthorTransfer()
		{
		}

		public AuthorTransfer(PrivateAccount signer, string name, Account newowner)
		{
			Signer = signer;
			Author = name;
			To = newowner;
		}

		public override bool IsValid()
		{
			return 0 < Author.Length;
		}

		public override void Read(BinaryReader r)
		{
			Author	= r.ReadUtf8();
			To= r.ReadAccount();
		}

		public override void Write(BinaryWriter w)
		{
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
		public ProductAddress	Name;
		public string			Title;
		public override string	Description => $"{Name} as {Title}";

		public ProductRegistration()
		{
		}

		public ProductRegistration(PrivateAccount signer, ProductAddress name, string title)
		{
			Signer		= signer;
			Name		= name;
			Title		= title;
		}

		public override bool IsValid()
		{
			return 0 < Name.Author.Length && 0 < Name.Product.Length;
		}

		public override void Read(BinaryReader r)
		{
			Name	= r.ReadProductAddress();
			Title	= r.ReadUtf8();
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Name);
			w.WriteUtf8(Title);
		}

		public override OperationResult Execute(Roundchain chain, Round round)
		{
			var a = round.FindAuthor(Name.Author);

			if(a == null || a.FindOwner(round) != Signer)
				return OperationResult.Failed;

			a.Products.Add(new Product {Name = Name.Product});
 
			var p = round.GetProduct(Name);
		
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

		public override bool IsValid()
		{
			return Product.Valid;
		}

		public override string	ToString()							=> base.ToString() + $", {Product}";
		public void				AddPublisher(Account publisher)		=> Actions[Change.AddPublisher] = publisher;
		public void				RemovePublisher(Account publisher)	=> Actions[Change.RemovePublisher] = publisher;
		public void				SetStatus(bool active)				=> Actions[Change.SetStatus] = active;

		public override void Read(BinaryReader r)
		{
			Product	= r.ReadProductAddress();
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
			w.Write(Product);
			w.Write(Actions, i =>	{
										w.Write((byte)i.Key);

										switch(i.Key)
										{
											case Change.AddPublisher:		w.Write(i.Value as Account); break;
											case Change.RemovePublisher:	w.Write(i.Value as Account); break;
											case Change.SetStatus:			w.Write((bool)i.Value); break;
										}; 
									});
		}
	}
}

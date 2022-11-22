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
		Null = 0, CandidacyDeclaration, Emission, UntTransfer, AuthorBid, AuthorRegistration, AuthorTransfer, ProductRegistration, RealizationRegistration , ReleaseRegistration 
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
		public Workflow			FlowReport;
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

		public virtual void Execute(Database chain, Round round)
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

			return Database.FeePerByte * ((Emission.FactorEnd - factor) / Emission.FactorEnd) * (int)s.Length;
		}
		
		public static Coin CalculateFee(Coin factor, IEnumerable<Operation> operations)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(operations, i => {
									 	i.WritePaid(w); 
									 });

			return Database.FeePerByte * ((Emission.FactorEnd - factor) / Emission.FactorEnd) * (int)s.Length;
		}

	}

	public class CandidacyDeclaration : Operation
	{
		public Coin				Bail;
		public IPAddress		IP;
		public override string	Description => $"{Bail} UNT";
		public override bool Valid => Settings.Dev.DisableBailMin ? true : Bail >= Database.BailMin;
		
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

		public override void Execute(Database chain, Round round)
		{
			var e = round.AffectAccount(Signer);

			var prev = e.ExeFindOperation<CandidacyDeclaration>(round);

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

		public override void Execute(Database chain, Round round)
		{
			Portion = Calculate(round.WeiSpent, round.Factor, Wei);
			
			if(Portion.Factor < FactorEnd)
			{
				round.AffectAccount(Signer).Balance += Portion.Amount;
				round.Distribute(Portion.Amount * 1/10, round.Funds);

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

		public override void  Execute(Database chain, Round round)
		{
			round.AffectAccount(Signer).Balance -= Amount;
			round.AffectAccount(To).Balance += Amount;
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

		public override void Execute(Database chain, Round round)
		{
			var a = round.AffectAuthor(Author);

			ChainTime sinceauction() => round.Time - a.FirstBidTime/* fb.Transaction.Payload.Round.Time*/;

			bool expired = a.LastWinner != null && (a.Owner == null && sinceauction() > ChainTime.FromYears(2) ||		/// winner has not registered since the end of auction																/// winner has not registered during 2 year since auction start, restart the auction
													a.Owner != null && round.Time - a.RegistrationTime > ChainTime.FromYears(a.Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			if(a.LastWinner == null || sinceauction() < ChainTime.FromYears(1))
	 			{
					if(a.LastWinner == null) /// first bid
					{
						round.AffectAccount(Signer).Balance -= Bid;
						
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
							round.AffectAccount(a.LastWinner).Balance += a.LastBid;
							round.AffectAccount(Signer).Balance -= Bid;
							
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
					round.AffectAccount(a.Owner).Authors.Remove(Author);
					a.Owner = null;
				}

				/// dont refund previous winner

				//var wb = a.FindLastBid(round);
				round.Distribute(a.LastBid, round.Members.Select(i => i.Generator), 1, round.Funds, 1);

				round.AffectAccount(Signer).Balance -= Bid;
				
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

		public override void Execute(Database chain, Round round)
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
						round.Distribute(a.LastBid, round.Members.Select(i => i.Generator), 1, round.Funds, 1);

					round.AffectAccount(Signer).Authors.Add(Author);
				}

				var cost = GetCost(round, Years);


				a = round.AffectAuthor(Author);
				a.ObtainedRid = round.Id;
				a.Title = Title;
				a.Owner = Signer;
				a.RegistrationTime = round.Time;
				a.Years = Years;

				round.AffectAccount(Signer).Balance -= cost;
				round.Distribute(cost, round.Members.Select(i => i.Generator), 1, round.Funds, 1);
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

		public override void Execute(Database chain, Round round)
		{
			if(!round.AffectAccount(Signer).Authors.Contains(Author))
			{
				Error = SignerDoesNotOwnTheAuthor;
				return;
			}

			round.AffectAccount(Signer).Authors.Remove(Author);
			round.AffectAccount(To).Authors.Add(Author);

			round.AffectAuthor(Author).ObtainedRid = round.Id;
			round.AffectAuthor(Author).Owner = To;
		}
	}


}

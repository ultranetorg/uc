using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UC.Net
{
	public class AuthorEntry : Entry<string>
	{
		public string				Name;
		public int					FirstBid = -1;
		public int					LastBid = -1;
		public int					LastRegistration = -1;
		public int					LastTransfer = -1;
		public HashSet<string>		Products = new();
		
		//public AuthorTransfer		LastTransferOperation => LastTransfer != -1 ? Chain.FindRound(LastTransfer).FindOperation<AuthorTransfer>(i => i.Author == Name) : null;
		//public Account				Owner => LastTransfer == -1 ? LastRegistrationOperation?.Signer : LastTransferOperation?.NewOwner;

		public override string		Key => Name;
		Roundchain					Chain;

		public AuthorEntry(Roundchain chain, string name)
		{
			Chain = chain;
			Name = name;
		}

		public AuthorEntry Clone()
		{
			return new AuthorEntry(Chain, Name)
					{ 
						FirstBid = FirstBid,
						LastBid = LastBid,
						LastRegistration = LastRegistration,
						LastTransfer = LastTransfer,
						Products = new HashSet<string>(Products)
					};
		}

		public override void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(FirstBid);
			w.Write7BitEncodedInt(LastBid);
			w.Write7BitEncodedInt(LastRegistration);
			w.Write7BitEncodedInt(LastTransfer);
			w.Write(Products, i => w.WriteUtf8(i));
		}

		public override void Read(BinaryReader r)
		{
			FirstBid		= r.Read7BitEncodedInt();
			LastBid			= r.Read7BitEncodedInt();
			LastRegistration= r.Read7BitEncodedInt();
			LastTransfer	= r.Read7BitEncodedInt();
			Products		= r.ReadHashSet(() => r.ReadUtf8());
		}

		public AuthorBid FindFirstBid(Round executing)
		{
			if(FirstBid != -1)
			{
				foreach(var b in Chain.FindRound(FirstBid).Payloads.AsEnumerable().Reverse())
					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
						foreach(var o in t.SuccessfulOperations.OfType<AuthorBid>().Reverse())
							if(o.Author == Name)
								return o;

				throw new IntegrityException("AuthorBid operation not found");
			}

			foreach(var r in Chain.Rounds.Where(i => i.Id < executing.Id).Reverse())
				foreach(var b in (r.Confirmed ? r.ConfirmedPayloads : r.Payloads).AsEnumerable().Reverse())
					foreach(var t in b.SuccessfulTransactions.AsEnumerable().Reverse())
						foreach(var o in t.SuccessfulOperations.AsEnumerable().Reverse())
							if(o is AuthorBid ab && ab.Author == Name)
								return ab;


			return executing.EffectiveOperations.Reverse().OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name);
		}

		public AuthorBid FindLastBid(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorBid>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorBid>(o => o.Author == Name && o.Result == OperationResult.OK, 
																t => t.Successful, 
																p => !p.Round.Confirmed || p.Confirmed, 
																r => r.Id < executing.Id)
					??
					(LastBid != -1 ? Chain.FindRound(LastBid).FindOperation<AuthorBid>(i => i.Author == Name) : null);
		}

		public AuthorRegistration FindRegistration(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorRegistration>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorRegistration>(o => o.Author == Name && o.Result == OperationResult.OK, 
																	t => t.Successful, 
																	p => !p.Round.Confirmed || p.Confirmed, 
																	r => r.Id < executing.Id)
					??
					(LastRegistration != -1 ? Chain.FindRound(LastRegistration).FindOperation<AuthorRegistration>(i => i.Author == Name) : null);
		}

		public AuthorTransfer FindTransfer(Round executing)
		{
			return	executing.EffectiveOperations.OfType<AuthorTransfer>().FirstOrDefault(i => i.Author == Name)
					??
					Chain.FindLastPoolOperation<AuthorTransfer>(o => o.Author == Name && o.Result == OperationResult.OK, 
																t => t.Successful, 
																p => !p.Round.Confirmed || p.Confirmed, 
																r => r.Id < executing.Id)
					??
					(LastTransfer != -1 ? Chain.FindRound(LastTransfer).FindOperation<AuthorTransfer>(i => i.Author == Name) : null);
		}

		public Account FindOwner(Round executing)
		{
			var r = FindRegistration(executing);
			var t = FindTransfer(executing);
		
			return t == null ? r?.Signer : t?.To;
		}

		public bool IsOngoingAuction(Round executing)
		{
			ChainTime sinceauction() => executing.Time - Chain.FindRound(FirstBid).Time/* fb.Transaction.Payload.Round.Time*/;

			bool expired = (LastRegistration == -1 && sinceauction() > ChainTime.FromYears(2) ||																			/// winner has not registered during 2 year since auction start, restart the auction
							LastRegistration != -1 && executing.Time - Chain.FindRound(LastRegistration).Time > ChainTime.FromYears(FindRegistration(executing).Years));	/// winner has not renewed, restart the auction

 			if(!expired)
 			{
	 			return sinceauction() < ChainTime.FromYears(1);
 			} 
 			else
 			{
				return true;
			}
		}
	}
}

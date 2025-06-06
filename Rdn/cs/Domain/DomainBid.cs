﻿namespace Uccs.Rdn;

public class DomainBid : RdnOperation
{
	public string			Name  { get; set; }
	public long				Bid  { get; set; }
	public override string	Explanation => $"{Bid} UNT for {Name}";
	
	public override bool IsValid(McvNet net)
	{
		if(!(net as Rdn).Auctions)
			return false;

		if(!Domain.IsWeb(Name))
			return false;

		if(Bid <= 0)
			return false;

		return true;
	} 

	public DomainBid()
	{
	}

	public DomainBid(string name, long bid)
	{
		Name = name;
		Bid = bid;
	}
	
	public override void Read(BinaryReader reader)
	{
		Name	= reader.ReadUtf8();
		Bid		= reader.Read7BitEncodedInt64();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Name);
		writer.Write7BitEncodedInt64(Bid);
	}

// 		public void WriteBaseState(BinaryWriter writer)
// 		{
// 			writer.Write(Id);
// 			writer.Write(Signer);
// 			writer.WriteUtf8(Name);
// 			writer.Write(Bid);
// 		}
// 
// 		public void ReadBaseState(BinaryReader reader)
// 		{
// 			_Id	= reader.Read<OperationId>();
// 
// 			Transaction = new Transaction();
// 			
// 			Transaction.Signer	= reader.ReadAccount();
// 			Name				= reader.ReadUtf8();
// 			Bid					= reader.Read<Money>();
// 		}

	public override void Execute(RdnExecution execution)
	{
		throw new NotImplementedException();

// 		var a = round.Domains.Affect(Name);
// 
//  		if(!Domain.IsExpired(a, execution.Time))
//  		{
// 			if(a.LastWinner == null) /// first bid
// 			{
// 				Signer.Spacetime -= Bid;
// 				
// 				a.Owner				= null;
// 				a.FirstBidTime		= execution.Time;
// 				a.LastBid			= Bid;
// 				a.LastBidTime		= execution.Time;
// 				a.LastWinner		= Signer.Id;
// 					
// 				EnergySpenders.Add(Signer);
// 				
// 				return;
// 			}
// 			else if(execution.Time < a.AuctionEnd)
// 			{
// 				if(a.LastBid < Bid) /// outbid
// 				{
// 					var lw = mcv.Accounts.Find(a.LastWinner, round.Id);
// 					
// 					round.AffectAccount(lw.Address).Spacetime += a.LastBid;
// 					Signer.Spacetime -= Bid;
// 					
// 					a.LastBid		= Bid;
// 					a.LastBidTime	= execution.Time;
// 					a.LastWinner	= Signer.Id;
// 
// 					EnergySpenders.Add(Signer);
// 					SpacetimeSpenders.Add(Signer);
// 					
// 					return;
// 				}
// 			}
//  		}
//  		else
//  		{
// 			/// dont refund previous winner if any
// 			///Transaction.BYReturned += a.LastBid;
// 
// 			Signer.Spacetime -= Bid;
// 			
// 			a.Owner				= null;
// 			a.FirstBidTime		= execution.Time;
// 			a.LastBid			= Bid;
// 			a.LastBidTime		= execution.Time;
// 			a.LastWinner		= Signer.Id;
// 		
// 			EnergySpenders.Add(Signer);
// 
// 			return;
// 		}
// 
// 		Error = "Bid too low or auction is over";
	}
}

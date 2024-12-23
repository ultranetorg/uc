namespace Uccs.Net;

public class AccountEntry : Account, ITableEntry
{
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }

	public bool				New;
	//public Unit			RoundBandwidthReserve = 0;
	//public Time			RoundBandwidthReserveExpiration = Time.Empty;
	
	//public List<int>		Transactions = new();
	Mcv						Mcv;

	public AccountEntry()
	{
	}

	public AccountEntry(Mcv mcv)
	{
		Mcv = mcv;
	}

	public virtual AccountEntry Clone()
	{
		var a = Mcv.Accounts.Create();

		a.Id						= Id;
		a.Address					= Address;
		a.BYBalance					= BYBalance;
		a.ECBalance					= ECBalance.ToList();
		a.LastTransactionNid		= LastTransactionNid;
		
		
		a.BandwidthNext				= BandwidthNext;
		a.BandwidthExpiration		= BandwidthExpiration;
		a.BandwidthToday			= BandwidthToday;
		a.BandwidthTodayTime		= BandwidthTodayTime;
		a.BandwidthTodayAvailable	= BandwidthTodayAvailable;

		return a;
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
	}

	public virtual void WriteMain(BinaryWriter w)
	{
		Write(w);
	}

	public virtual void ReadMain(BinaryReader r)
	{
		Read(r);
	}

	public void WriteMore(BinaryWriter w)
	{
		//if(Mcv.Settings.Chain != null)
		//{
		//	w.Write(Transactions, w.Write7BitEncodedInt);
		//}
	}

	public void ReadMore(BinaryReader r)
	{
		//if(Mcv.Settings.Chain != null)
		//{
		//	Transactions = r.ReadList(r.Read7BitEncodedInt);
		//}
	}

	public void Cleanup(Round lastInCommit)
	{
		ECBalance?.RemoveAll(i => i.Expiration < lastInCommit.ConsensusTime);
	}
}

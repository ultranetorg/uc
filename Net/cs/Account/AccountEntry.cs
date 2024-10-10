namespace Uccs.Net
{
	public class AccountEntry : Account, ITableEntry<AccountAddress>
	{
		public AccountAddress			Key => Address;
		
		//[JsonIgnore]
		public bool						New;
		//public Unit						RoundBandwidthReserve = 0;
		//public Time						RoundBandwidthReserveExpiration = Time.Empty;
		
		public HashSet<int>				Transactions = new();
		Mcv								Mcv;

		public AccountEntry()
		{
		}

		public AccountEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public AccountEntry Clone()
		{
			return new AccountEntry(Mcv){	Id						= Id,
											Address					= Address,
											BYBalance				= BYBalance,
											ECBalance				= ECBalance.ToList(),
											LastTransactionNid		= LastTransactionNid,
											LastEmissionId			= LastEmissionId,
											
											BandwidthNext			= BandwidthNext,
											BandwidthExpiration		= BandwidthExpiration,
											BandwidthToday			= BandwidthToday,
											BandwidthTodayTime		= BandwidthTodayTime,
											BandwidthTodayAvailable	= BandwidthTodayAvailable,

											Transactions			= Mcv.Settings.Chain != null ? new HashSet<int>(Transactions) : null};
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);

		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
		}

		public void WriteMain(BinaryWriter w)
		{
			Write(w);
		}

		public void ReadMain(BinaryReader r)
		{
			Read(r);
		}

		public void WriteMore(BinaryWriter w)
		{
			if(Mcv.Settings.Chain != null)
			{
				w.Write(Transactions);
			}
		}

		public void ReadMore(BinaryReader r)
		{
			if(Mcv.Settings.Chain != null)
			{
				Transactions = r.ReadHashSet(() => r.Read7BitEncodedInt());
			}
		}

		public void Cleanup(Round lastInCommit)
		{
			ECBalance?.RemoveAll(i => i.Expiration < lastInCommit.ConsensusTime);
		}
	}
}

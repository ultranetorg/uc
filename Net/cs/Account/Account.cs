using System.IO;

namespace Uccs.Net
{
	public class Account : IBinarySerializable
	{
		public EntityId			Id { get; set; }
		public AccountAddress	Address { get; set; }
		public Unit				BYBalance { get; set; }
		public Unit				ECBalance { get; set; }
		public Unit				MRBalance { get; set; }
		public int				LastTransactionNid { get; set; } = -1;
		public int				LastEmissionId  { get; set; } = -1;
		public Unit				AverageUptime { get; set; }
		
		public Unit				BandwidthNext { get; set; }
		public Time				BandwidthExpiration { get; set; } = Time.Empty;
		public Unit				BandwidthToday { get; set; }
		public Time				BandwidthTodayTime { get; set; }
		public Unit				BandwidthTodayAvailable { get; set; }

		public virtual void Write(BinaryWriter writer)
		{
			writer.Write(Address);
			writer.Write(ECBalance);
			writer.Write(BYBalance);
			writer.Write(MRBalance);
			writer.Write7BitEncodedInt(LastTransactionNid);
			writer.Write7BitEncodedInt(LastEmissionId);
			writer.Write(AverageUptime);
			
			writer.Write(BandwidthNext);
			
			if(BandwidthNext > 0)
			{
				writer.Write(BandwidthExpiration);
				writer.Write(BandwidthToday);
				writer.Write(BandwidthTodayTime);
				writer.Write(BandwidthTodayAvailable);
			}
		}

		public virtual void Read(BinaryReader reader)
		{
			Address				= reader.ReadAccount();
			ECBalance 			= reader.Read<Unit>();
			BYBalance 			= reader.Read<Unit>();
			MRBalance 			= reader.Read<Unit>();
			LastTransactionNid	= reader.Read7BitEncodedInt();
			LastEmissionId		= reader.Read7BitEncodedInt();
			AverageUptime		= reader.Read<Unit>();

			BandwidthNext = reader.Read<Unit>();

			if(BandwidthNext > 0)
			{
				BandwidthExpiration		= reader.Read<Time>();
				BandwidthToday			= reader.Read<Unit>();
				BandwidthTodayTime		= reader.Read<Time>();
				BandwidthTodayAvailable	= reader.Read<Unit>();
			}
		}
	}
}

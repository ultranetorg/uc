using System.Diagnostics;

namespace Uccs.Fair
{
	public class PublisherEntry : Publisher, ITableEntry
	{
		public bool				New;
		public bool				Affected;
		Mcv						Mcv;

		public PublisherEntry()
		{
		}

		public PublisherEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public override string ToString()
		{
			return $"{Id}, {Owner}, {Expiration}";
		}

		public PublisherEntry Clone()
		{
			return new PublisherEntry(Mcv) {Id = Id,
											Owner = Owner,
											Expiration = Expiration,
											SpaceReserved = SpaceReserved,
											SpaceUsed = SpaceUsed,
											};
		}

		public void WriteMain(BinaryWriter writer)
		{
			var f = PublisherFlag.None;

			writer.Write((byte)f);
			writer.Write7BitEncodedInt(SpaceReserved);
			writer.Write7BitEncodedInt(SpaceUsed);

			writer.Write(Owner);
			writer.Write(Expiration);

		}

		public void Cleanup(Round lastInCommit)
		{
		}

		public void ReadMain(BinaryReader reader)
		{
			var f			= (PublisherFlag)reader.ReadByte();
			SpaceReserved	= (short)reader.Read7BitEncodedInt();
			SpaceUsed		= (short)reader.Read7BitEncodedInt();

			Owner		= reader.Read<EntityId>();
			Expiration	= reader.Read<Time>();
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

	}
}

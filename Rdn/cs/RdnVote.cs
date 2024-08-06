namespace Uccs.Rdn
{
	public class RdnVote : Vote
	{
		//public ForeignResult[]		Emissions = {};
		public ForeignResult[]		Migrations = {};

		public RdnVote(Mcv mcv) : base(mcv)
		{
		}

		protected override void WriteVote(BinaryWriter writer)
		{
			base.WriteVote(writer);

			//writer.Write(Emissions);
			writer.Write(Migrations);
		}

		protected override void ReadVote(BinaryReader reader)
		{
			base.ReadVote(reader);

			//Emissions	= reader.ReadArray<ForeignResult>();
			Migrations	= reader.ReadArray<ForeignResult>();
		}
	}
}

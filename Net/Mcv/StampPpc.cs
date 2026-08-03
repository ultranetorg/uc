namespace Uccs.Net;

public class StampPpc : McvPpc<StampPpr>
{
	public override Result Execute()
	{
		lock(Mcv.Lock)
		{
			RequireGraph();

			var r = new StampPpr
					{
						GraphState				= Mcv.GraphState,
						GraphHash				= Mcv.GraphHash,
						LastCommitedRound		= Mcv.LastCommitedRound?.Id ?? -1,
						LastCommitedRoundHash	= Mcv.LastCommitedRound?.Hash,
						Tables					= Mcv.Tables.Select(i => new StampPpr.Table
																		 {
																			Id = i.Id, 
																			Clusters = i.Clusters.Select(i =>	new StampPpr.Cluster
																												{
																													Id = i.Id, 
																													Hash = i.Hash
																												}).ToArray()
																		 }).ToArray()
					};

			return r;
		}
	}

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}
}

public class StampPpr : Result
{
	public class Cluster
	{
		public short	Id { get; set; }
		public byte[]	Hash { get; set; }
	}

	public class Table
	{
		public int			Id { get; set; }
		public Cluster[]	Clusters { get; set; }
	}

	public byte[]		GraphState { get; set; }
	public byte[]		GraphHash { get; set; }
	public int			LastCommitedRound { get; set; }
	public byte[]		LastCommitedRoundHash { get; set; }
	public Table[]		Tables { get; set; }

	public override void Write(Writer writer)
	{
		writer.WriteBytes(GraphState);
		writer.Write(GraphHash);
		writer.Write7BitEncodedInt(LastCommitedRound);
		writer.Write(LastCommitedRoundHash);
		writer.Write(Tables, i =>	{
										writer.Write7BitEncodedInt(i.Id);
										writer.Write(i.Clusters, i =>	{
																			writer.Write(i.Id);
																			writer.Write(i.Hash);
																		});
									});
	}

	public override void Read(Reader reader)
	{
		GraphState				= reader.ReadBytes();
		GraphHash				= reader.ReadHash();
		LastCommitedRound		= reader.Read7BitEncodedInt();
		LastCommitedRoundHash	= reader.ReadHash();
		Tables					= reader.ReadArray(() =>	new Table
															{
																Id = reader.Read7BitEncodedInt(), 
																Clusters = reader.ReadArray(() =>	new Cluster
																									{
																										Id = reader.ReadInt16(), 
																										Hash = reader.ReadHash()
																									})
															});
	}
}

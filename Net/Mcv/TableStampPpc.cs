namespace Uccs.Net;

public class TableStampPpc : McvPpc<TableStampPpr>
{
	public byte		Table { get; set; }
	public short[]	Clusters { get; set; }

	public override Result Execute()
	{
		if(Clusters.Length > TableBase.ClustersCountMax)
			throw new RequestException(RequestError.IncorrectRequest);
				
		RequireGraph();
		
		lock(Mcv.Lock)
		{
			if(Mcv.GraphState == null)
				throw new NodeException(NodeError.TooEearly);

			if(Table < 0 || Mcv.Tables.Length <= Table)
				throw new RequestException(RequestError.OutOfRange);

			return new TableStampPpr {Clusters = Clusters.Select(i =>	{			
																			var c = Mcv.Tables[Table].FindCluster(i);

																			var r = new TableStampPpr.Cluster
																					{
																						Id = c.Id,
																						Buckets = c.Buckets.Select(i => new TableStampPpr.Bucket  {Id = i.Id, 
																																					//Length = i.Size, 
																																					Hash = i.Hash}).ToArray()
																							
																					};
																			return r;
																		})
																		.ToArray()};
			
		}
	}

	public override void Read(Reader reader)
	{
		Table = reader.ReadByte();
		Clusters = reader.ReadArray(reader.ReadInt16);
	}

	public override void Write(Writer writer)
	{
		writer.Write(Table);
		writer.Write(Clusters, writer.Write);
	}

}

public class TableStampPpr : Result
{
	public class Bucket
	{
		public int		Id { get; set; }
		public byte[]	Hash { get; set; }
	}

	public class Cluster
	{
		public short	Id { get; set; }
		public Bucket[]	Buckets { get; set; }
	}

	public Cluster[]	Clusters { get; set; }

	public override void Write(Writer writer)
	{
		writer.Write(Clusters, i =>	{
										writer.Write(i.Id);
										writer.Write(i.Buckets, i =>	{
																			writer.Write7BitEncodedInt(i.Id);
																			writer.Write(i.Hash);
																		});
									});
	}

	public override void Read(Reader reader)
	{
		Clusters = reader.ReadArray(() =>	new Cluster
											{
												Id = reader.ReadInt16(), 
												Buckets = reader.ReadArray(() =>	new Bucket
																					{
																						Id = reader.Read7BitEncodedInt(), 
																						Hash = reader.ReadHash()
																					})
											});
	}

}

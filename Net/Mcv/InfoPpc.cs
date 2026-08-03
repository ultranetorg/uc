
namespace Uccs.Net;

public class InfoPpc : McvPpc<InfoPpr>
{
	public override Result Execute()
	{
		RequireGraph();		

		return	new InfoPpr
				{
					Tables = Mcv.Tables.ToDictionary(i => i.Id, i => i.Name),
					Assets =	[	
									Asset.Spacetime,
									Asset.Energy(0, Node.Mcv.LastConfirmedRound.ConsensusTime.Years),
									Asset.Energy(1, (byte)(Node.Mcv.LastConfirmedRound.ConsensusTime.Years + 1))
								]

				};
	}

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}
}

public class InfoPpr : Result
{
	public Dictionary<byte, string>		Tables { get; set; }
	public Asset[]						Assets { get; set; }

	public override void Read(Reader reader)
	{
		Tables = reader.ReadDictionary(() => reader.ReadByte(), () => reader.ReadASCII());
		Assets = reader.ReadArray<Asset>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Tables, i => writer.Write(i),  i => writer.WriteASCII(i));
		writer.Write(Assets);
	}
}

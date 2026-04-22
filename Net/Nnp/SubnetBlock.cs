namespace Uccs.Net;

public class SubnetBlock
{
	public string		Name { get; set; }
	public SubnetState	State { get; set; }

	byte[]				_RawPayload;

	public byte[] RawPayload
	{
		get
		{ 
			if(_RawPayload == null)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				WritePayload(w);

				_RawPayload = s.ToArray();
			}
		
			return _RawPayload; 
		}

		set { _RawPayload = value; }
	}

	public void WritePayload(BinaryWriter writer)
	{
		writer.WriteASCII(Name);
		writer.Write(State);
	}

	public void ReadPayload(BinaryReader reader)
	{
		Name	= reader.ReadASCII();
		State	= reader.Read<SubnetState>();
	}

	public void Restore()
	{
		ReadPayload(new BinaryReader(new MemoryStream(RawPayload)));
	}
}

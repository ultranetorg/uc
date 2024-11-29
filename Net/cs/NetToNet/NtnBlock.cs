﻿namespace Uccs.Net
{
	public class NtnBlock
	{
		public string		Net { get; set; }
		public NtnState		State { get; set; }

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
			writer.WriteUtf8(Net);
			writer.Write(State);
		}

		public void ReadPayload(BinaryReader reader)
		{
			Net		= reader.ReadUtf8();
			State	= reader.Read<NtnState>();
		}

		public void Restore()
		{
			ReadPayload(new BinaryReader(new MemoryStream(RawPayload)));
		}
	}
}

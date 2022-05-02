using System.Collections.Generic;
using System.IO;
using RocksDbSharp;

namespace UC.Net
{
	public enum MessageType : byte
	{
		Null, ReleaseDeclaration
	}
	
	public abstract class Message : ITypedBinarySerializable
	{
		public abstract bool	Valid {get;}
		public byte				BinaryType => (byte)Type;
		public ProcessingStage	Stage;	
		public Account			Signer;
		public Proposition		Proposition;
	
		public static Message FromType(MessageType type)
		{
			return type switch
						{
							MessageType.ReleaseDeclaration	=> new ReleaseDeclaration(),
							_								=> throw new IntegrityException("Wrong MessageType"),
						};
		}
	
		public MessageType Type
		{
			get
			{
				return this switch
							{
								ReleaseDeclaration	=> MessageType.ReleaseDeclaration,
								_					=> throw new IntegrityException("Wrong MessageType"),
							};
			}
		}
	
		public abstract void Read(BinaryReader r);
		public abstract void Write(BinaryWriter w);
	}
	
	public class ReleaseDeclaration : Message, IHashable
	{
		public ReleaseAddress			Address { get;set; }
		public string					Channel { get;set; }		/// stable, beta, nightly, debug,...

		public override bool			Valid => Address.Valid;

		public ReleaseDeclaration()
		{
		}

		public ReleaseDeclaration(ReleaseAddress address, string channel)
		{
			Address = address;
			Channel = channel;
		}
		
		public void HashWrite(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Channel);
		}

		public override void Read(BinaryReader r)
		{
			Address = r.ReadReleaseAddress();
			Channel = r.ReadUtf8();
		}

		public override void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Channel);
		}
	}
}

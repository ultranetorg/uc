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
//							MessageType.ReleaseDeclaration	=> new ReleaseDeclaration(),
							_								=> throw new IntegrityException("Wrong MessageType"),
						};
		}
	
		public MessageType Type
		{
			get
			{
				return this switch
							{
//								ReleaseDeclaration	=> MessageType.ReleaseDeclaration,
								_					=> throw new IntegrityException("Wrong MessageType"),
							};
			}
		}
	
		public abstract void Read(BinaryReader r);
		public abstract void Write(BinaryWriter w);
	}

}
